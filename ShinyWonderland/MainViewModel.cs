using System.Reactive.Disposables;
using Humanizer;
using Shiny.Notifications;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public partial class MainViewModel(
    ILogger<MainViewModel> logger,
    IMediator mediator,
    IOptions<ParkOptions> parkOptions,
    AppSettings appSettings,
    TimeProvider timeProvider,
    IGpsManager gpsManager,
    IGeofenceManager geofenceManager,
    INotificationManager notifications,
    INavigator navigation
) : ObservableObject, INavigatedAware, IConnectivityEventHandler, IEventHandler<JobDataRefreshEvent>
{
    CancellationTokenSource? cancellationTokenSource;
    CompositeDisposable? disposer;
    
    [ObservableProperty] public partial IReadOnlyList<RideInfo> Rides { get; private set; } = null!;
    [ObservableProperty] public partial bool IsBusy { get; private set; }
    [ObservableProperty] public partial string? DataTimestamp { get; private set; }

    [NotifyPropertyChangedFor(nameof(IsNotConnected))]
    [ObservableProperty]
    public partial bool IsConnected { get; private set; }
    public bool IsNotConnected => !IsConnected;
    
    public string Title => parkOptions.Value.Name;
    
    [RelayCommand] Task NavToSettings() => navigation.NavigateTo("SettingsPage");
    [RelayCommand] Task NavToParking() => navigation.NavigateTo("ParkingPage");
    

    public async void OnNavigatedTo()
    {
        this.LoadData(false).RunInBackground(logger);
        
        await notifications.RequestAccess();
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
            var access = await gpsManager.RequestAccess(GpsRequest.Realtime(true));
            if (access == AccessState.Available && gpsManager.CurrentListener == null)
            {
                var start = await gpsManager.IsWithinPark(parkOptions.Value);
                if (start)
                    await gpsManager.StartListener(GpsRequest.Realtime(true));
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
            var exists = geofenceManager.GetMonitorRegions().Any(x => x.Identifier == "Wonderland");
            if (!exists)
            {
                await geofenceManager.StartMonitoring(new GeofenceRegion(
                    "Wonderland",
                    parkOptions.Value.CenterOfPark,
                    Distance.FromKilometers(1)
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
            var result = await mediator.GetWonderlandData(forceRefresh, this.cancellationTokenSource.Token);
            this.StartDataTimer(result.Context.Cache()?.Timestamp);
            
            var rides = result
                .Result
                .LiveData
                .Where(x => x.EntityType == EntityType.ATTRACTION)
                .Select(x => new RideInfo(
                    x.Name,
                    x.Status == LiveStatusType.OPERATING ? x.Queue?.Standby?.WaitTime : null,
                    x.Status == LiveStatusType.OPERATING ? x.Queue?.PaidStandby?.WaitTime : null,
                    x.Status == LiveStatusType.OPERATING
                    // x.Queue?.PaidReturnTime?.Price?.Formatted
                    // x.Queue?.PaidReturnTime?.Price?.Formatted + " " x.Queue?.PaidReturnTime?.Price?.Currency
                ));

            if (appSettings.ShowOpenOnly)
                rides = rides.Where(x => x.IsOpen);

            if (appSettings.ShowTimedOnly)
                rides = rides.Where(x => x.HasPaidWaitTime || x.HasWaitTime);
            
            switch (appSettings.Ordering)
            {
                case RideOrder.Name:
                    rides = rides.OrderBy(x => x.Name);
                    break;
                
                case RideOrder.WaitTime:
                    rides = rides
                        .OrderBy(x => x.WaitTimeMinutes ?? 999) // nulls are moved to end of the list
                        .ThenBy(x => x.Name); 
                    break;
                
                case RideOrder.PaidWaitTime:
                    rides = rides
                        .OrderBy(x => x.PaidWaitTimeMinutes ?? 999) // nulls are moved to end of the list
                        .ThenBy(x => x.Name);
                    break;
            }
            this.Rides = rides.ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error loading data");
            await navigation.Alert("ERROR", "There was an error loading the data");
        }
        finally
        {
            this.IsBusy = false;
        }
    }


    void StartDataTimer(DateTimeOffset? from)
    {
        this.disposer?.Dispose(); // get rid of original timer if exists from pull-to-refresh
        this.disposer = new();
        from ??= timeProvider.GetUtcNow();
        this.DataTimestamp = from.Value.Humanize();
        
        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => from.Value.Humanize())
            .Subscribe(x => this.DataTimestamp = x)
            .DisposedBy(this.disposer);
    }
}


public record RideInfo(
    string Name,
    int? WaitTimeMinutes,
    int? PaidWaitTimeMinutes,
    bool IsOpen
)
{
    public bool IsClosed => !this.IsOpen;
    public bool HasWaitTime => this.WaitTimeMinutes.HasValue;
    public bool HasPaidWaitTime => this.PaidWaitTimeMinutes.HasValue;
};