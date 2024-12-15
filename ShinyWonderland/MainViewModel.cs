using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public partial class MainViewModel(
    IMediator mediator,
    ILogger<MainViewModel> logger
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] IReadOnlyList<RideInfo> rides = null!;
    [ObservableProperty] bool isBusy;

    public void OnAppearing() =>  this.LoadData(false).RunInBackground(logger);
    public void OnDisappearing() {}
    [RelayCommand] Task Load() => this.LoadData(true);


    async Task LoadData(bool forceRefresh)
    {
        try
        {
            this.IsBusy = true;
            var result = await mediator.GetWonderlandData(forceRefresh, CancellationToken.None);
            this.Rides = result
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
}


public record RideInfo(
    string Name, 
    // DateTime OpenTime,
    // DateTime CloseTime,
    int WaitTimeMinutes, 
    int? PaidWaitTimeMinutes,
    string? PaidAmount
);