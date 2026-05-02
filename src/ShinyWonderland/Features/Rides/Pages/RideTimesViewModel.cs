using System.Reactive.Subjects;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Features.Rides.Pages;


[ShellMap<RideTimesPage>(registerRoute: false)]
public partial class RideTimesViewModel(
    ViewModelServices services,
    ILogger<RideTimesViewModel> logger,
    Humanizer humanizer
) : BaseViewModel(services),
    IEventHandler<JobDataRefreshEvent>,
    IEventHandler<GpsEvent>
{
    Position? currentPosition;

    public override string Title => services.ParkOptions.Value.Name;
    [ObservableProperty] public partial IReadOnlyList<RideTimeViewModel> Rides { get; private set; } = [];
    [ObservableProperty] public partial string? DataTimestamp { get; private set; }

    public override void OnAppearing()
    {
        this.LoadData(false).RunInBackground(logger);

        this.gpsEventSubj
            .Sample(TimeSpan.FromSeconds(3))
            .Where(x => this.Rides.Any(r => r.DistanceMeters != null)) // only update if we have at least one ride with a known distance
            .Subscribe(@event => Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                var rides = this.Rides.ToList();
                foreach (var ride in rides)
                    ride.UpdateDistance(@event.Position);

                this.currentPosition = @event.Position;
                if (services.AppSettings.Ordering == RideOrder.Distance)
                {
                    this.Rides = rides
                        .OrderBy(x => x.DistanceMeters ?? 999)
                        .ThenBy(x => x.Name)
                        .ToList();
                }
            }))
            .DisposedBy(this.DeactivateWith);
    }
    

    [RelayCommand]
    Task GoToHistory() => services.Navigator.NavigateToRideHistory();

    [RelayCommand]
    Task Load() => this.LoadData(true);
    
    [MainThread]
    public Task Handle(JobDataRefreshEvent @event, IMediatorContext context, CancellationToken cancellationToken)
        => this.LoadData(false);
    
    readonly Subject<GpsEvent> gpsEventSubj = new();
    public Task Handle(GpsEvent @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        logger.LogDebug("Received GPS event with position: {Position}", @event.Position);
        this.gpsEventSubj.OnNext(@event);

        return Task.CompletedTask;
    }
    
    async Task LoadData(bool forceRefresh)
    {
        try
        {
            var result = await services.Mediator.Request(
                new GetCurrentRideTimes(), 
                this.DeactivateToken,
                ctx =>
                {
                    if (forceRefresh)
                        ctx.ForceCacheRefresh();
                }
            );
            this.StartDataTimer(result.Context.Cache()?.Timestamp);
            this.FilterSortBind(result.Result);
            this.IsBusy = false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error loading data");
            await services.Dialogs.Alert(Localize.Error, Localize.GeneralError, Localize.Ok);
            this.IsBusy = false;
        }
    }


    void FilterSortBind(List<RideTime> rides)
    {
        var query = rides
            .Select(x =>
            {
                var vm = new RideTimeViewModel(x, humanizer, services);
                if (this.currentPosition != null)
                    vm.UpdateDistance(this.currentPosition);
                
                return vm;
            });
        
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
        from ??= services.TimeProvider.GetUtcNow();
        this.DataTimestamp = humanizer.TimeAgo(from.Value);
        
        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => humanizer.TimeAgo(from.Value))
            .Subscribe(x => this.DataTimestamp = x)
            .DisposedBy(this.DeactivateWith);
    }
}


public partial class RideTimeViewModel(
    RideTime rideTime,
    Humanizer humanizer,
    ViewModelServices services
) : ObservableObject
{
    public string Name => rideTime.Name;
    public StringsLocalized Localize => services.Localized;
    public int? WaitTimeMinutes => rideTime.WaitTimeMinutes;
    public int? PaidWaitTimeMinutes => rideTime.PaidWaitTimeMinutes;
    public bool IsOpen => rideTime.IsOpen;
    public bool IsClosed => !rideTime.IsOpen;
    public bool HasWaitTime => rideTime.WaitTimeMinutes.HasValue;
    public bool HasPaidWaitTime => rideTime.PaidWaitTimeMinutes.HasValue;
    public bool HasLastRide => LastRidden != null;
    
    DateTimeOffset? lastRidden;
    public string? LastRidden => humanizer.TimeAgo(lastRidden ?? rideTime.LastRidden);
    
    [ObservableProperty] string distanceText = services.Localized.UnknownDistance;
    [ObservableProperty] double? distanceMeters;
    
    [RelayCommand]
    async Task AddRide()
    {
        var confirm = await services.Dialogs.Confirm(
            services.Localized.HistoryDialogTitle, 
            services.Localized.AddRideHistoryQuestionFormat(this.Name)
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
