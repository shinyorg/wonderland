using ShinyWonderland.Contracts;

namespace ShinyWonderland.Tests.ViewModels;

public class MapRideTimesViewModelTests
{
    readonly TestMediator mediator;
    readonly IOptions<ParkOptions> parkOptions;
    readonly MapRideTimesViewModel viewModel;

    public MapRideTimesViewModelTests()
    {
        mediator = new TestMediator();

        var options = new ParkOptions
        {
            Name = "Test Park",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000,
            MapStartZoomDistanceMeters = 500
        };
        parkOptions = Options.Create(options);

        viewModel = new MapRideTimesViewModel(mediator, parkOptions);
    }

    [Test]
    public async Task OnAppearing_ShouldLoadOpenRidesWithPositions()
    {
        var rides = new List<RideTime>
        {
            new("1", "Open Ride", 30, 10, new Position(33.8121, -117.9190), true, null),
            new("2", "Closed Ride", null, null, new Position(33.8122, -117.9191), false, null),
            new("3", "No Position Ride", 20, 5, null, true, null),
            new("4", "Another Open Ride", 15, 5, new Position(33.8123, -117.9192), true, null)
        };

        mediator.SetupRequest<GetCurrentRideTimes, List<RideTime>>(rides);

        viewModel.OnAppearing();
        await Task.Delay(100);

        await Assert.That(viewModel.Rides).IsNotNull();
        await Assert.That(viewModel.Rides.Count).IsEqualTo(2); // Only open rides with positions
        await Assert.That(viewModel.Rides.All(r => r.Text.Contains("Wait:"))).IsTrue();
    }

    [Test]
    public async Task CenterOfPark_ShouldReturnParkOptionsPosition()
    {
        await Assert.That(viewModel.CenterOfPark.Latitude).IsEqualTo(parkOptions.Value.Latitude);
        await Assert.That(viewModel.CenterOfPark.Longitude).IsEqualTo(parkOptions.Value.Longitude);
    }

    [Test]
    public async Task MapStartZoomDistanceMeters_ShouldReturnParkOptionsValue()
    {
        await Assert.That(viewModel.MapStartZoomDistanceMeters).IsEqualTo(parkOptions.Value.MapStartZoomDistanceMeters);
    }

    [Test]
    public async Task OnAppearing_WithNoOpenRides_ShouldReturnEmptyList()
    {
        var rides = new List<RideTime>
        {
            new("1", "Closed Ride 1", null, null, new Position(33.8121, -117.9190), false, null),
            new("2", "Closed Ride 2", null, null, new Position(33.8122, -117.9191), false, null)
        };

        mediator.SetupRequest<GetCurrentRideTimes, List<RideTime>>(rides);

        viewModel.OnAppearing();
        await Task.Delay(100);

        await Assert.That(viewModel.Rides).IsNotNull();
        await Assert.That(viewModel.Rides.Count).IsEqualTo(0);
    }

    [Test]
    public async Task MapItem_ShouldContainFormattedText()
    {
        var mapItem = new MapItem(
            "Test Ride\nWait: 30 min\nPaid Wait: 10 min",
            new Microsoft.Maui.Devices.Sensors.Location(33.8121, -117.9190)
        );

        await Assert.That(mapItem.Text).Contains("Test Ride");
        await Assert.That(mapItem.Text).Contains("Wait: 30 min");
        await Assert.That(mapItem.Text).Contains("Paid Wait: 10 min");
        await Assert.That(mapItem.Location.Latitude).IsEqualTo(33.8121);
    }

    [Test]
    public void OnDisappearing_ShouldNotThrow()
    {
        viewModel.OnDisappearing();
    }
}
