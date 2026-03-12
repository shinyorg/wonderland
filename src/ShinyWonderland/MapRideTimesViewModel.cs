using Microsoft.Maui.Maps;
using ShinyWonderland.Contracts;

namespace ShinyWonderland;


[ShellMap<MapRideTimesPage>(registerRoute: false)]
public partial class MapRideTimesViewModel(
    IMediator mediator,
    IOptions<ParkOptions> parkOptions
) : ObservableObject, IPageLifecycleAware
{

    [ObservableProperty] List<MapItem> rides;
    public Position CenterOfPark => parkOptions.Value.CenterOfPark;
    public int MapStartZoomDistanceMeters => parkOptions.Value.MapStartZoomDistanceMeters;
    
    public async void OnAppearing()
    {
        // TODO: timer refresh & changes
        var result = await mediator.Request(new GetCurrentRideTimes());
        this.Rides = result.Result
            .Where(x => x is { IsOpen: true, Position: not null })
            .Select(x => new MapItem(
                $"{x.Name}\nWait: {x.WaitTimeMinutes} min\nPaid Wait: {x.PaidWaitTimeMinutes} min",
                new Location(x.Position!.Latitude, x.Position!.Longitude)
            ))
            .ToList();
    }

    
    public void OnDisappearing()
    {
    }
}

public record MapItem(
    string Text, 
    Location Location
);