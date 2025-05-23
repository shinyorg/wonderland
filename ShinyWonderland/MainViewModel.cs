using System.Reactive.Disposables;
using Humanizer;
using Shiny.Locations;
using Shiny.Notifications;
using ShinyWonderland.Services;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public partial class MainViewModel(
    IMediator mediator,
    IConfiguration config,
    AppSettings appSettings,
    IGpsManager gpsManager,
    TimeProvider timeProvider,
    INotificationManager notifications,
    INavigator navigation,
    ILogger<MainViewModel> logger
) : ObservableObject, INavigatedAware, IConnectivityEventHandler
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
    
    
    [RelayCommand] Task NavToSettings() => navigation.NavigateTo("SettingsPage");
    [RelayCommand] Task NavToParking() => navigation.NavigateTo("ParkingPage");
    

    public async void OnNavigatedTo()
    {
        this.LoadData(false).RunInBackground(logger);
        
        await notifications.RequestAccess();
        var access = await gpsManager.RequestAccess(GpsRequest.Realtime(true));
        if (access == AccessState.Available)
            await gpsManager.StartListener(GpsRequest.Realtime(true));
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

    public string Title => config.GetValue<string>("Park:Name")!;
    
    async Task LoadData(bool forceRefresh)
    {
        try
        {
            this.cancellationTokenSource = new();
            this.IsBusy = true;
            var result = await mediator.GetWonderlandData(forceRefresh, this.cancellationTokenSource.Token);
            this.StartDataTimer(result.Context.Cache()?.Timestamp);
            // var operatingHours = await mediator.Request(new GetEntityScheduleUpcomingHttpRequest());
            // operatingHours.Result.Schedule.FirstOrDefault(x => x.OpeningTime.Date == )
            
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
                    rides = rides.OrderBy(x => x.WaitTimeMinutes);
                    break;
            }
            this.Rides = rides.ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error loading data");
        }
        finally
        {
            this.IsBusy = false;
        }
    }
    
    
    [MainThread]
    public Task Handle(ConnectivityChanged @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        this.IsConnected = @event.Connected;
        return Task.CompletedTask;
    }


    void StartDataTimer(DateTimeOffset? from)
    {
        this.disposer = new();
        from ??= timeProvider.GetUtcNow();
        this.DataTimestamp = from.Value.Humanize();
        
        Observable
            .Interval(TimeSpan.FromSeconds(10))
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