using Humanizer;
using Shiny.Locations;
using Shiny.Notifications;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public partial class MainViewModel(
    IMediator mediator,
    IConfiguration config,
    IGpsManager gpsManager,
    INotificationManager notifications,
    INavigationService navigation,
    ILogger<MainViewModel> logger
) : ObservableObject, IPageLifecycleAware, IApplicationLifecycleAware, IConnectivityEventHandler
{
    CancellationTokenSource? cancellationTokenSource;
    
    [ObservableProperty] public partial IReadOnlyList<RideInfo> Rides { get; private set; } = null!;
    [ObservableProperty] public partial bool IsBusy { get; private set; }
    [ObservableProperty] public partial bool IsConnected { get; private set; }
    [ObservableProperty] public partial string? CacheTime { get; private set; }
    [RelayCommand] Task NavToSettings() =>navigation.NavigateAsync(nameof(SettingsPage));

    public void OnResume() => this.LoadData(false).RunInBackground(logger);
    public void OnSleep() => this.cancellationTokenSource?.Cancel();

    public async void OnAppearing()
    {
        this.LoadData(false).RunInBackground(logger);
        
        // TODO: permissions
        await notifications.RequestAccess();
        // await gpsManager.RequestAccess(GpsRequest.Foreground); background permission
    }

    public void OnDisappearing() => this.cancellationTokenSource?.Cancel();


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
            var cacheInfo = result.Context.Cache();
            this.CacheTime = cacheInfo?.IsHit == true
                ? cacheInfo.Timestamp.Humanize()
                : null;

            // var operatingHours = await mediator.Request(new GetEntityScheduleUpcomingHttpRequest());
            // operatingHours.Result.Schedule.FirstOrDefault(x => x.OpeningTime.Date == )
            
            this.Rides = result
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
                ))
                .OrderBy(x => x.Name)
                .ToList();
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
}


public record RideInfo(
    string Name,
    int? WaitTimeMinutes, 
    int? PaidWaitTimeMinutes,
    bool IsOpen
);