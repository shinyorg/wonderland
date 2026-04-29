namespace ShinyWonderland.Features.Rides.Pages;


[ShellMap<MapRideTimesPage>(registerRoute: false)]
public partial class MapRideTimesViewModel(ViewModelServices services) : BaseViewModel(services)
{

    [ObservableProperty] List<MapItem> rides;
    public Position CenterOfPark => services.ParkOptions.Value.CenterOfPark;
    public int MapStartZoomDistanceMeters => services.ParkOptions.Value.MapStartZoomDistanceMeters;
    
    public override async void OnAppearing()
    {
        try
        {
            // TODO: timer refresh & changes
            var result = await Mediator.Request(new GetCurrentRideTimes());
            this.Rides = result.Result
                .Where(x => x is { IsOpen: true, Position: not null })
                .Select(x => new MapItem(
                    $"{x.Name}\nWait: {x.WaitTimeMinutes} min\nPaid Wait: {x.PaidWaitTimeMinutes} min",
                    new Location(x.Position!.Latitude, x.Position!.Longitude)
                ))
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load ride map data");
        }
    }
}

public record MapItem(
    string Text, 
    Location Location
);
