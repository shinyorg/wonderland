namespace ShinyWonderland.Tests.ViewModels;

public class ParkingViewModelTests
{
    readonly CoreServices services;
    readonly IMediaPicker mediaPicker;
    readonly ILogger<ParkingViewModel> logger;
    readonly ParkingViewModelLocalized localize;
    readonly AppSettings appSettings;
    readonly IGpsManager gpsManager;
    readonly INavigator navigator;
    readonly ParkingViewModel viewModel;

    public ParkingViewModelTests()
    {
        mediaPicker = Substitute.For<IMediaPicker>();
        logger = Substitute.For<ILogger<ParkingViewModel>>();
        localize = Substitute.For<ParkingViewModelLocalized>();

        localize.SetParking.Returns("Set Parking Location");
        localize.RemoveParking.Returns("Remove Parking Location");
        localize.PermissionDenied.Returns("Permission Denied");
        localize.OpenSettings.Returns("Open Settings?");
        localize.Reset.Returns("Reset");
        localize.ConfirmReset.Returns("Are you sure you want to reset?");
        localize.Error.Returns("Error");
        localize.NotCloseEnough.Returns("Not close enough to park");
        localize.ErrorRetrievingLocation.Returns("Error retrieving location");

        appSettings = new AppSettings();
        gpsManager = Substitute.For<IGpsManager>();
        navigator = Substitute.For<INavigator>();

        var parkOptions = new ParkOptions
        {
            Name = "Test Park",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000,
            MapStartZoomDistanceMeters = 500
        };

        var mediator = Substitute.For<IMediator>();
        var notifications = Substitute.For<INotificationManager>();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        services = new CoreServices(
            mediator,
            Options.Create(parkOptions),
            appSettings,
            navigator,
            timeProvider,
            gpsManager,
            notifications
        );

        viewModel = new ParkingViewModel(services, mediaPicker, logger, localize);
    }

    [Fact]
    public void CommandText_WhenNotParked_ShouldReturnSetParking()
    {
        // Assert
        viewModel.ParkLocation.ShouldBeNull();
        viewModel.CommandText.ShouldBe("Set Parking Location");
    }

    [Fact]
    public void CommandText_WhenParked_ShouldReturnRemoveParking()
    {
        // Arrange
        viewModel.ParkLocation = new Position(33.8121, -117.9190);

        // Assert
        viewModel.CommandText.ShouldBe("Remove Parking Location");
    }

    [Fact]
    public void IsParked_WhenNoParkLocation_ShouldBeFalse()
    {
        // Assert
        viewModel.IsParked.ShouldBeFalse();
    }

    [Fact]
    public void IsParked_WhenParkLocationSet_ShouldBeTrue()
    {
        // Arrange
        viewModel.ParkLocation = new Position(33.8121, -117.9190);

        // Assert
        viewModel.IsParked.ShouldBeTrue();
    }

    [Fact]
    public void HasParkedImage_WhenNoImage_ShouldBeFalse()
    {
        // Assert
        viewModel.HasParkedImage.ShouldBeFalse();
    }

    [Fact]
    public void HasParkedImage_WhenImageSet_ShouldBeTrue()
    {
        // Arrange
        viewModel.ImageUri = "file://some/path.png";

        // Assert
        viewModel.HasParkedImage.ShouldBeTrue();
    }

    [Fact]
    public void CenterOfPark_ShouldReturnParkOptionsPosition()
    {
        // Assert
        viewModel.CenterOfPark.Latitude.ShouldBe(33.8121);
        viewModel.CenterOfPark.Longitude.ShouldBe(-117.9190);
    }

    [Fact]
    public void MapStartZoomDistanceMeters_ShouldReturnParkOptionsValue()
    {
        // Assert
        viewModel.MapStartZoomDistanceMeters.ShouldBe(500);
    }

    [Fact]
    public void OnAppearing_ShouldLoadParkingLocationFromAppSettings()
    {
        // Arrange
        var expectedPosition = new Position(33.8121, -117.9190);
        appSettings.ParkingLocation = expectedPosition;

        // Act
        viewModel.OnAppearing();

        // Assert
        viewModel.ParkLocation.ShouldBe(expectedPosition);
    }

    [Fact]
    public void OnAppearing_WhenNoParkingLocation_ShouldHaveNullParkLocation()
    {
        // Arrange
        appSettings.ParkingLocation = null;

        // Act
        viewModel.OnAppearing();

        // Assert
        viewModel.ParkLocation.ShouldBeNull();
    }

    [Fact]
    public void Localize_ShouldReturnInjectedLocalize()
    {
        // Assert
        viewModel.Localize.ShouldBe(localize);
    }

    [Fact]
    public void OnDisappearing_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => viewModel.OnDisappearing());
    }

    [Fact]
    public void IsBusy_ShouldStartAsFalse()
    {
        // Assert
        viewModel.IsBusy.ShouldBeFalse();
    }
}