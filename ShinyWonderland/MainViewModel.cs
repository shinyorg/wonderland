using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public partial class MainViewModel(IMediator mediator) : ObservableObject
{
    const string WonderlandEntityId = "66f5d97a-a530-40bf-a712-a6317c96b06d";
    [ObservableProperty] IReadOnlyList<RideInfo> rides = null!;
    [ObservableProperty] bool isBusy;
    

    [RelayCommand]
    async Task Load(CancellationToken cancellationToken)
    {
        // TODO: search filter and Timer Auto Refresh 
        var result = await mediator.Request(
            new GetEntityLiveDataHttpRequest { EntityID = WonderlandEntityId }, 
            cancellationToken
        );
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
}


public record RideInfo(
    string Name, 
    // DateTime OpenTime,
    // DateTime CloseTime,
    int WaitTimeMinutes, 
    int? PaidWaitTimeMinutes,
    string? PaidAmount
);