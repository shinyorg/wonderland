namespace ShinyWonderland.Tests.ViewModels;

public class MapRideTimesViewModelTests
{
    readonly IMediator mediator;
    readonly IOptions<ParkOptions> parkOptions;
    readonly MapRideTimesViewModel viewModel;

    public MapRideTimesViewModelTests()
    {
        mediator = Substitute.For<IMediator>();

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

    [Fact]
    public async Task OnAppearing_ShouldLoadOpenRidesWithPositions()
    {
        // Arrange
        var rides = new List<RideTime>
        {
            new("1", "Open Ride", 30, 10, new Position(33.8121, -117.9190), true, null),
            new("2", "Closed Ride", null, null, new Position(33.8122, -117.9191), false, null),
            new("3", "No Position Ride", 20, 5, null, true, null),
            new("4", "Another Open Ride", 15, 5, new Position(33.8123, -117.9192), true, null)
        };

        mediator.Request(Arg.Any<GetCurrentRideTimes>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(rides));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100);

        // Assert
        viewModel.Rides.ShouldNotBeNull();
        viewModel.Rides.Count.ShouldBe(2); // Only open rides with positions
        viewModel.Rides.ShouldAllBe(r => r.Text.Contains("Wait:"));
    }

    [Fact]
    public void CenterOfPark_ShouldReturnParkOptionsPosition()
    {
        // Assert
        viewModel.CenterOfPark.Latitude.ShouldBe(parkOptions.Value.Latitude);
        viewModel.CenterOfPark.Longitude.ShouldBe(parkOptions.Value.Longitude);
    }

    [Fact]
    public void MapStartZoomDistanceMeters_ShouldReturnParkOptionsValue()
    {
        // Assert
        viewModel.MapStartZoomDistanceMeters.ShouldBe(parkOptions.Value.MapStartZoomDistanceMeters);
    }

    [Fact]
    public async Task OnAppearing_WithNoOpenRides_ShouldReturnEmptyList()
    {
        // Arrange
        var rides = new List<RideTime>
        {
            new("1", "Closed Ride 1", null, null, new Position(33.8121, -117.9190), false, null),
            new("2", "Closed Ride 2", null, null, new Position(33.8122, -117.9191), false, null)
        };

        mediator.Request(Arg.Any<GetCurrentRideTimes>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(rides));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100);

        // Assert
        viewModel.Rides.ShouldNotBeNull();
        viewModel.Rides.Count.ShouldBe(0);
    }

    [Fact]
    public void MapItem_ShouldContainFormattedText()
    {
        // Arrange
        var mapItem = new MapItem(
            "Test Ride\nWait: 30 min\nPaid Wait: 10 min",
            new Microsoft.Maui.Devices.Sensors.Location(33.8121, -117.9190)
        );

        // Assert
        mapItem.Text.ShouldContain("Test Ride");
        mapItem.Text.ShouldContain("Wait: 30 min");
        mapItem.Text.ShouldContain("Paid Wait: 10 min");
        mapItem.Location.Latitude.ShouldBe(33.8121);
    }

    [Fact]
    public void OnDisappearing_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => viewModel.OnDisappearing());
    }
}