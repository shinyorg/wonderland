using System.Reactive.Disposables;
using Humanizer;
using ShinyWonderland.Contracts;
using ShinyWonderland.Handlers;

namespace ShinyWonderland;


[ShellMap<RideTimesPage>(registerRoute: false)]
public partial class RideTimesViewModel(
    CoreServices services,
    ILogger<RideTimesViewModel> logger,
    IGeofenceManager geofenceManager,
    RideTimesViewModelLocalized localize
) : 
    ObservableObject, 
    IPageLifecycleAware, 
    IConnectivityEventHandler, 
    IEventHandler<JobDataRefreshEvent>,
    IEventHandler<GpsEvent>
{
    CancellationTokenSource? cancellationTokenSource;
    CompositeDisposable? disposer;
    Position? currentPosition;

    public RideTimesViewModelLocalized Localize => localize;
    public string Title => services.ParkOptions.Value.Name;
    [ObservableProperty] public partial IReadOnlyList<RideTimeViewModel> Rides { get; private set; } = null!;
    [ObservableProperty] public partial bool IsBusy { get; private set; }
    [ObservableProperty] public partial string? DataTimestamp { get; private set; }

    [NotifyPropertyChangedFor(nameof(IsNotConnected))]
    [ObservableProperty]
    public partial bool IsConnected { get; private set; }
    public bool IsNotConnected => !IsConnected;
    
    
    public async void OnAppearing()
    {
        this.LoadData(false).RunInBackground(logger);
        
        await services.Notifications.RequestAccess();
        await this.TryGps();
        await this.TryGeofencing();
    }

    
    public void OnDisappearing()
    {
        this.cancellationTokenSource?.Cancel();
        this.disposer?.Dispose();
    }


    [RelayCommand]
    Task GoToHistory() => services.Navigator.NavigateToRideHistory();

    [RelayCommand]
    async Task Load()
    {
        if (!this.IsBusy)
            await this.LoadData(true);
    }

    
    [MainThread]
    public Task Handle(ConnectivityChanged @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        this.IsConnected = @event.Connected;
        return Task.CompletedTask;
    }
    
    
    [MainThread]
    public Task Handle(JobDataRefreshEvent @event, IMediatorContext context, CancellationToken cancellationToken)
        => this.LoadData(false);

    
    [MainThread]
    public Task Handle(GpsEvent @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        logger.LogDebug("Received GPS event with position: {Position}", @event.Position);
        foreach (var ride in this.Rides)
            ride.UpdateDistance(@event.Position);
        
        this.currentPosition = @event.Position;
        if (services.AppSettings.Ordering == RideOrder.Distance)
        {
            this.Rides = this.Rides
                .OrderBy(x => x.DistanceMeters ?? 999)
                .ThenBy(x => x.Name)
                .ToList();
        }

        return Task.CompletedTask;
    }
    

    async Task TryGps()
    {
        try
        {
            var access = await services.Gps.RequestAccess(GpsRequest.Realtime(true));
            
            // only check GPS if background is running and user has granted permissions
            if (access == AccessState.Available && services.Gps.CurrentListener == null)
            {
                var start = await services.IsUserWithinPark();
                if (start)
                    await services.Gps.StartListener(GpsRequest.Realtime(true));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start GPS");
        }
    }


    const string GEOFENCE_ID = "ThemePark";
    async Task TryGeofencing()
    {
        var access = await geofenceManager.RequestAccess();
        if (access == AccessState.Available)
        {
            var regions = geofenceManager.GetMonitorRegions();
            if (!regions.Any(x => x.Identifier.Equals(GEOFENCE_ID, StringComparison.InvariantCultureIgnoreCase)))
            {
                await geofenceManager.StartMonitoring(new GeofenceRegion(
                    GEOFENCE_ID,
                    services.ParkOptions.Value.CenterOfPark,
                    services.ParkOptions.Value.NotificationDistance
                ));
            }
        }
    }
    
    
    async Task LoadData(bool forceRefresh)
    {
        try
        {
            this.cancellationTokenSource = new();
            this.IsBusy = true;
            
            var result = await services.Mediator.Request(
                new GetCurrentRideTimes(), 
                this.cancellationTokenSource.Token,
                ctx =>
                {
                    if (forceRefresh)
                        ctx.ForceCacheRefresh();
                }
            );
            this.StartDataTimer(result.Context.Cache()?.Timestamp);
            this.FilterSortBind(result.Result);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error loading data");
            await services.Navigator.Alert(localize.Error, localize.GeneralError, localize.Ok);
        }
        finally
        {
            this.IsBusy = false;
        }
    }


    void FilterSortBind(List<RideTime> rides)
    {
        var query = rides
            .Select(x =>
            {
                var vm = new RideTimeViewModel(x, localize, services);
                if (this.currentPosition != null)
                    vm.UpdateDistance(this.currentPosition);
                
                return vm;
            })
            .AsQueryable();
        
        logger.LogDebug("Received {Count} rides from API", query.Count());
        if (services.AppSettings.ShowOpenOnly)
        {
            logger.LogDebug("Adding open only filter");
            query = query.Where(x => x.IsOpen);
        }

        if (services.AppSettings.ShowTimedOnly)
        {
            logger.LogDebug("Adding timed only filter");
            query = query.Where(x => x.PaidWaitTimeMinutes != null || x.WaitTimeMinutes != null);
        }

        switch (services.AppSettings.Ordering)
        {
            case RideOrder.Name:
                query = query.OrderBy(x => x.Name);
                break;
                
            case RideOrder.WaitTime:
                query = query
                    .OrderBy(x => x.WaitTimeMinutes ?? 999) // nulls are moved to end of the list
                    .ThenBy(x => x.Name); 
                break;
                
            case RideOrder.PaidWaitTime:
                query = query
                    .OrderBy(x => x.PaidWaitTimeMinutes ?? 999) // nulls are moved to end of the list
                    .ThenBy(x => x.Name);
                break;
            
            case RideOrder.Distance:
                query = query
                    .OrderBy(x => x.DistanceMeters ?? 999)
                    .ThenBy(x => x.Name);
                break;
        }
        this.Rides = query.ToList();
        logger.LogDebug("Rides Output: {Count}", this.Rides.Count);
    }
    

    void StartDataTimer(DateTimeOffset? from)
    {
        this.disposer?.Dispose(); // get rid of original timer if exists from pull-to-refresh
        this.disposer = new();
        from ??= services.TimeProvider.GetUtcNow();
        this.DataTimestamp = from.Value.LocalDateTime.Humanize();
        
        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => from.Value.Humanize())
            .Subscribe(x => this.DataTimestamp = x)
            .DisposedBy(this.disposer);
    }
}


public partial class RideTimeViewModel(
    RideTime rideTime,
    RideTimesViewModelLocalized localize,
    CoreServices services
) : ObservableObject
{
    public string Name => rideTime.Name;
    public RideTimesViewModelLocalized Localize => localize;
    public int? WaitTimeMinutes => rideTime.WaitTimeMinutes;
    public int? PaidWaitTimeMinutes => rideTime.PaidWaitTimeMinutes;
    public bool IsOpen => rideTime.IsOpen;
    public bool IsClosed => !rideTime.IsOpen;
    public bool HasWaitTime => rideTime.WaitTimeMinutes.HasValue;
    public bool HasPaidWaitTime => rideTime.PaidWaitTimeMinutes.HasValue;
    public bool HasLastRide => LastRidden != null;
    
    DateTimeOffset? lastRidden;
    public string? LastRidden => (lastRidden ?? rideTime.LastRidden)?.ToLocalTime().Humanize();
    
    [ObservableProperty] string distanceText = localize.UnknownDistance;
    [ObservableProperty] double? distanceMeters;
    
    [RelayCommand]
    async Task AddRide()
    {
        var confirm = await services.Navigator.Confirm(
            localize.HistoryDialogTitle, 
            String.Format(localize.AddRideHistoryQuestion, this.Name)
        );
        if (confirm)
        {
            await services.Mediator.Send(new AddRideCommand(rideTime.Id, this.Name));
            this.lastRidden = services.TimeProvider.GetUtcNow();
            this.OnPropertyChanged(nameof(this.LastRidden));
            this.OnPropertyChanged(nameof(this.HasLastRide));
        }
    }
    
    
    public void UpdateDistance(Position position)
    {
        if (rideTime.Position == null)
            return;

        var dist = rideTime.Position.GetDistanceTo(position);
        this.DistanceMeters = Math.Round(dist.TotalMeters, 0);
        if (dist.TotalMeters > 1000)
        {
            var km = Math.Round(dist.TotalKilometers, 1);
            this.DistanceText = $"{km} km";
        }
        else
        {
            this.DistanceText = $"{this.DistanceMeters} m";
        }
    }
}