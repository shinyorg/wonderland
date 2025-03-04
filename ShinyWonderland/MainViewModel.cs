using Humanizer;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public partial class MainViewModel(
    IMediator mediator,
    ILogger<MainViewModel> logger
) : ObservableObject, IPageLifecycleAware, IConnectivityEventHandler
{
    [ObservableProperty] public partial IReadOnlyList<RideInfo> Rides { get; private set; } = null!;
    [ObservableProperty] public partial bool IsBusy { get; private set; }
    [ObservableProperty] public partial bool IsConnected { get; private set; }
    [ObservableProperty] public partial string? CacheTime { get; private set; }

    public void OnAppearing() => this.LoadData(false).RunInBackground(logger);
    public void OnDisappearing() { }
    

    // always take from cache, user has to pull to refresh to force update?
    // could force refresh if fresh start of app?
    [RelayCommand] Task Load() => this.LoadData(false);
    public string Title => Constants.ParkName;

    async Task LoadData(bool forceRefresh)
    {
        try
        {
            this.IsBusy = true;
            var result = await mediator.GetWonderlandData(forceRefresh, CancellationToken.None);
            var cacheInfo = result.Context.Cache();
            this.CacheTime = cacheInfo?.IsHit == true
                ? cacheInfo.Timestamp.Humanize()
                : null;
            
            this.Rides = result
                .Result
                .LiveData
                .Where(x =>
                    x.EntityType == EntityType.ATTRACTION &&
                    (
                        x.Queue?.Standby?.WaitTime != null ||
                        x.Queue?.PaidStandby?.WaitTime != null
                    )
                )
                .Select(x => new RideInfo(
                    x.Name,
                    //x.OperatingHours?.FirstOrDefault(x => x.Type == EntityType.)
                    x.Queue?.Standby?.WaitTime ?? 0,
                    x.Queue?.PaidStandby?.WaitTime,
                    x.Queue?.PaidReturnTime?.Price?.Formatted
                    // x.Queue?.PaidReturnTime?.Price?.Formatted + " " x.Queue?.PaidReturnTime?.Price?.Currency
                ))
                .OrderBy(x => x.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            // TODO: System.Threading.Tasks.TaskCanceledException on httpclient timeout
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
    }
}


public record RideInfo(
    string Name, 
    // DateTime OpenTime,
    // DateTime CloseTime,
    int WaitTimeMinutes, 
    int? PaidWaitTimeMinutes,
    string? PaidAmount
);