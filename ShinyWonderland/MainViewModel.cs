using System.Reactive.Disposables;
using Humanizer;
using ShinyWonderland.Contracts;

namespace ShinyWonderland;


public partial class MainViewModel(
    CoreServices services,
    ILogger<MainViewModel> logger,
    IGeofenceManager geofenceManager
) : 
    ObservableObject, 
    INavigatedAware, 
    IConnectivityEventHandler, 
    IEventHandler<JobDataRefreshEvent>,
    IEventHandler<GpsEvent>
{
    CancellationTokenSource? cancellationTokenSource;
    CompositeDisposable? disposer;
    
    public string Title => services.ParkOptions.Value.Name;
    [ObservableProperty] public partial IReadOnlyList<RideTimeViewModel> Rides { get; private set; } = null!;
    [ObservableProperty] public partial bool IsBusy { get; private set; }
    [ObservableProperty] public partial string? DataTimestamp { get; private set; }

    [NotifyPropertyChangedFor(nameof(IsNotConnected))]
    [ObservableProperty]
    public partial bool IsConnected { get; private set; }
    public bool IsNotConnected => !IsConnected;
    
    [RelayCommand] Task NavToSettings() => services.Navigator.NavigateTo("SettingsPage");
    [RelayCommand] Task NavToParking() => services.Navigator.NavigateTo("ParkingPage");
    

    public async void OnNavigatedTo()
    {
        this.LoadData(false).RunInBackground(logger);
        
        await services.Notifications.RequestAccess();
        await this.TryGps();
        await this.TryGeofencing();
    }

    
    public void OnNavigatedFrom()
    {
        this.cancellationTokenSource?.Cancel();
        this.disposer?.Dispose();
    }


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
    

    async Task TryGeofencing()
    {
        var access = await geofenceManager.RequestAccess();
        if (access == AccessState.Available)
        {
            var exists = geofenceManager
                .GetMonitorRegions()
                .Any(x => x.Identifier == "Wonderland");
            
            if (!exists)
            {
                await geofenceManager.StartMonitoring(new GeofenceRegion(
                    "Wonderland",
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
            await services.Navigator.Alert("ERROR", "There was an error loading the data");
        }
        finally
        {
            this.IsBusy = false;
        }
    }


    void FilterSortBind(List<RideTime> rides)
    {
        var query = rides.AsQueryable();
        if (services.AppSettings.ShowOpenOnly)
            query = query.Where(x => x.IsOpen);

        if (services.AppSettings.ShowTimedOnly)
            query = query.Where(x => x.PaidWaitTimeMinutes != null || x.WaitTimeMinutes != null);
            
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
        }
        this.Rides = query
            .Select(x => new RideTimeViewModel(x))
            .ToList();
    }
    

    void StartDataTimer(DateTimeOffset? from)
    {
        this.disposer?.Dispose(); // get rid of original timer if exists from pull-to-refresh
        this.disposer = new();
        from ??= services.TimeProvider.GetUtcNow();
        this.DataTimestamp = from.Value.Humanize();
        
        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => from.Value.Humanize())
            .Subscribe(x => this.DataTimestamp = x)
            .DisposedBy(this.disposer);
    }

    
    [MainThread]
    public async Task Handle(GpsEvent @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        
        // TODO: each ride needs to have their coordinates on them
        // TODO: iterate displayed rides and set distance on them
        // TODO: if sorted by distance also trigger a rebind to list
    }
}


public partial class RideTimeViewModel(RideTime rideTime) : ObservableObject
{
    public string Name => rideTime.Name;
    public int? WaitTimeMinutes => rideTime.WaitTimeMinutes;
    public int? PaidWaitTimeMinutes => rideTime.PaidWaitTimeMinutes;
    public bool IsClosed => !rideTime.IsOpen;
    public bool HasWaitTime => rideTime.WaitTimeMinutes.HasValue;
    public bool HasPaidWaitTime => rideTime.PaidWaitTimeMinutes.HasValue;

    [ObservableProperty] string distanceText;

    public void UpdateDistance(Position position)
    {
        if (rideTime.Position == null)
            return;

        var dist = rideTime.Position.GetDistanceTo(position);
        if (dist.TotalKilometers > 2)
        {
            this.DistanceText = "TOO FAR";
        }
        else
        {
            var meters = Math.Round(dist.TotalMeters, 0);
            this.DistanceText = $"{meters} m";
        }
    }
}