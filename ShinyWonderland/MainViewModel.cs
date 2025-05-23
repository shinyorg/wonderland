using System.Runtime.CompilerServices;
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
    INotificationManager notifications,
    INavigator navigation,
    ILogger<MainViewModel> logger
) : ObservableObject, INavigatedAware, IConnectivityEventHandler
{
    CancellationTokenSource? cancellationTokenSource;
    
    [ObservableProperty] public partial IReadOnlyList<RideInfo> Rides { get; private set; } = null!;
    [ObservableProperty] public partial bool IsBusy { get; private set; }

    [NotifyPropertyChangedFor(nameof(IsNotConnected))]
    [ObservableProperty]
    public partial bool IsConnected { get; private set; }
    public bool IsNotConnected => !IsConnected;
    
    [NotifyPropertyChangedFor(nameof(IsFromCache))]
    [ObservableProperty]
    public partial string? CacheTime { get; private set; }
    public bool IsFromCache => !this.CacheTime.IsEmpty();
    
    [RelayCommand] Task NavToSettings() => navigation.NavigateTo(nameof(SettingsPage));

    // public void OnResume() => this.LoadData(false).RunInBackground(logger);
    // public void OnSleep() => this.cancellationTokenSource?.Cancel();

    public async void OnNavigatedTo()
    {
        this.LoadData(false).RunInBackground(logger);
        
        await notifications.RequestAccess();
        var access = await gpsManager.RequestAccess(GpsRequest.Realtime(true));
        if (access == AccessState.Available)
            await gpsManager.StartListener(GpsRequest.Realtime(true));
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