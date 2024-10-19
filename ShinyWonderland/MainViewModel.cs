using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public partial class MainViewModel(IMediator mediator) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] IReadOnlyList<RideInfo> rides = null!;
    [ObservableProperty] bool isBusy;


    public void OnAppearing()
    {
        this.LoadCommand.Execute(null);
    }

    public void OnDisappearing()
    {
    }


    [RelayCommand]
    async Task Load(CancellationToken cancellationToken)
    {
        // TODO: search filter and Timer Auto Refresh 
        try
        {
            this.IsBusy = true;
            var result = await mediator.GetWonderlandData(cancellationToken);
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
            Console.WriteLine(ex);
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