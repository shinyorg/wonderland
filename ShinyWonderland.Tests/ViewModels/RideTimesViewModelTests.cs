namespace ShinyWonderland.Tests.ViewModels;

public class RideTimesViewModelTests
{
    readonly CoreServices services;
    readonly ILogger<RideTimesViewModel> logger;
    readonly IGeofenceManager geofenceManager;
    readonly Humanizer humanizer;
    readonly RideTimesViewModelLocalized localize;
    readonly AppSettings appSettings;
    readonly IGpsManager gpsManager;
    readonly IMediator mediator;
    readonly INavigator navigator;
    readonly FakeTimeProvider timeProvider;
    readonly INotificationManager notifications;

    public RideTimesViewModelTests()
    {
        logger = Substitute.For<ILogger<RideTimesViewModel>>();
        geofenceManager = Substitute.For<IGeofenceManager>();
        localize = Substitute.For<RideTimesViewModelLocalized>();

        localize.Error.Returns("Error");
        localize.GeneralError.Returns("A general error occurred");
        localize.Ok.Returns("OK");
        localize.UnknownDistance.Returns("Unknown");

        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var humanizerLocalized = Substitute.For<HumanizerLocalized>();
        humanizerLocalized.Never.Returns("Never");
        humanizerLocalized.Second.Returns("second");
        humanizerLocalized.Seconds.Returns("seconds");
        humanizerLocalized.Minute.Returns("minute");
        humanizerLocalized.Minutes.Returns("minutes");
        humanizerLocalized.Hour.Returns("hour");
        humanizerLocalized.Hours.Returns("hours");

        humanizer = new Humanizer(timeProvider, humanizerLocalized);

        appSettings = new AppSettings();
        gpsManager = Substitute.For<IGpsManager>();
        navigator = Substitute.For<INavigator>();
        mediator = Substitute.For<IMediator>();
        notifications = Substitute.For<INotificationManager>();

        var parkOptions = new ParkOptions
        {
            Name = "Wonderland",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000,
            MapStartZoomDistanceMeters = 500
        };

        services = new CoreServices(
            mediator,
            Options.Create(parkOptions),
            appSettings,
            navigator,
            timeProvider,
            gpsManager,
            notifications
        );
    }

    RideTimesViewModel CreateViewModel() => new(
        services,
        logger,
        geofenceManager,
        humanizer,
        localize
    );

    [Fact]
    public void Title_ShouldReturnParkName()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        vm.Title.ShouldBe("Wonderland");
    }

    [Fact]
    public void Localize_ShouldReturnInjectedLocalize()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        vm.Localize.ShouldBe(localize);
    }

    [Fact]
    public void Rides_ShouldStartEmpty()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        vm.Rides.ShouldNotBeNull();
        vm.Rides.Count.ShouldBe(0);
    }

    [Fact]
    public void IsBusy_ShouldStartFalse()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        vm.IsBusy.ShouldBeFalse();
    }

    // [Fact]
    // public void IsNotConnected_ShouldBeInverseOfIsConnected()
    // {
    //     // Arrange
    //     var vm = CreateViewModel();
    //
    //     // Assert - default state
    //     vm.IsConnected.ShouldBeFalse();
    //     vm.IsNotConnected.ShouldBeTrue();
    //
    //     // Act
    //     vm.IsConnected = true;
    //
    //     // Assert
    //     vm.IsNotConnected.ShouldBeFalse();
    // }

    [Fact]
    public async Task Handle_ConnectivityChanged_ShouldUpdateIsConnected()
    {
        // Arrange
        var vm = CreateViewModel();
        var context = Substitute.For<IMediatorContext>();

        // Act
        await vm.Handle(new ConnectivityChanged(true), context, CancellationToken.None);

        // Assert
        vm.IsConnected.ShouldBeTrue();
    }

    // [Fact]
    // public async Task Handle_GpsEvent_ShouldUpdateRideDistances()
    // {
    //     // Arrange
    //     var vm = CreateViewModel();
    //
    //     // Setup initial rides
    //     var rides = new List<RideTime>
    //     {
    //         new("1", "Test Ride", 30, 10, new Position(33.8122, -117.9191), true, null)
    //     };
    //
    //     mediator.Request(Arg.Any<GetCurrentRideTimes>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
    //         .Returns(MediatorTestHelpers.CreateResult(rides));
    //     notifications.RequestAccess(Arg.Any<CancellationToken>()).Returns(AccessState.Available);
    //     gpsManager.RequestAccess(Arg.Any<GpsRequest>(), Arg.Any<CancellationToken>()).Returns(AccessState.Available);
    //     geofenceManager.RequestAccess(Arg.Any<CancellationToken>()).Returns(AccessState.Available);
    //
    //     vm.OnAppearing();
    //     await Task.Delay(150);
    //
    //     var context = Substitute.For<IMediatorContext>();
    //     var gpsEvent = new GpsEvent(new Position(33.8123, -117.9192));
    //
    //     // Act
    //     await vm.Handle(gpsEvent, context, CancellationToken.None);
    //
    //     // Assert - distance should be updated (not "Unknown" anymore)
    //     vm.Rides.ShouldNotBeEmpty();
    // }

    [Fact]
    public void OnDisappearing_ShouldNotThrow()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act & Assert
        Should.NotThrow(() => vm.OnDisappearing());
    }
}

public class RideTimeViewModelTests
{
    readonly RideTimesViewModelLocalized localize;
    readonly Humanizer humanizer;
    readonly CoreServices services;
    readonly FakeTimeProvider timeProvider;

    public RideTimeViewModelTests()
    {
        localize = Substitute.For<RideTimesViewModelLocalized>();
        localize.UnknownDistance.Returns("Unknown");
        localize.HistoryDialogTitle.Returns("Add to History");
        localize.AddRideHistoryQuestionFormat(Arg.Any<string>()).Returns(x => $"Add {x[0]} to history?");

        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var humanizerLocalized = Substitute.For<HumanizerLocalized>();
        humanizerLocalized.Never.Returns("Never");
        humanizerLocalized.Seconds.Returns("seconds");
        humanizerLocalized.Minutes.Returns("minutes");

        humanizer = new Humanizer(timeProvider, humanizerLocalized);

        var appSettings = new AppSettings();
        var gpsManager = Substitute.For<IGpsManager>();
        var navigator = Substitute.For<INavigator>();
        var mediator = Substitute.For<IMediator>();
        var notifications = Substitute.For<INotificationManager>();

        var parkOptions = new ParkOptions
        {
            Name = "Test Park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000
        };

        services = new CoreServices(
            mediator,
            Options.Create(parkOptions),
            appSettings,
            navigator,
            timeProvider,
            gpsManager,
            notifications
        );
    }

    [Fact]
    public void Name_ShouldReturnRideName()
    {
        // Arrange
        var ride = new RideTime("1", "Space Mountain", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        // Assert
        vm.Name.ShouldBe("Space Mountain");
    }

    [Fact]
    public void WaitTimeMinutes_ShouldReturnRideWaitTime()
    {
        // Arrange
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        // Assert
        vm.WaitTimeMinutes.ShouldBe(30);
        vm.PaidWaitTimeMinutes.ShouldBe(10);
    }

    [Fact]
    public void IsOpen_ShouldReflectRideStatus()
    {
        // Arrange
        var openRide = new RideTime("1", "Open Ride", 30, 10, null, true, null);
        var closedRide = new RideTime("2", "Closed Ride", null, null, null, false, null);

        var openVm = new RideTimeViewModel(openRide, localize, humanizer, services);
        var closedVm = new RideTimeViewModel(closedRide, localize, humanizer, services);

        // Assert
        openVm.IsOpen.ShouldBeTrue();
        openVm.IsClosed.ShouldBeFalse();
        closedVm.IsOpen.ShouldBeFalse();
        closedVm.IsClosed.ShouldBeTrue();
    }

    [Fact]
    public void HasWaitTime_ShouldBeTrue_WhenWaitTimeHasValue()
    {
        // Arrange
        var ride = new RideTime("1", "Test Ride", 30, null, null, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        // Assert
        vm.HasWaitTime.ShouldBeTrue();
        vm.HasPaidWaitTime.ShouldBeFalse();
    }

    [Fact]
    public void HasLastRide_ShouldBeFalse_WhenNeverRidden()
    {
        // Arrange
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        // Assert
        vm.HasLastRide.ShouldBeFalse();
    }

    [Fact]
    public void HasLastRide_ShouldBeTrue_WhenPreviouslyRidden()
    {
        // Arrange
        var lastRidden = DateTimeOffset.UtcNow.AddHours(-1);
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, lastRidden);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        // Assert
        vm.HasLastRide.ShouldBeTrue();
        vm.LastRidden.ShouldNotBeNull();
    }

    [Fact]
    public void DistanceText_ShouldDefaultToUnknown()
    {
        // Arrange
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        // Assert
        vm.DistanceText.ShouldBe("Unknown");
        vm.DistanceMeters.ShouldBeNull();
    }

    [Fact]
    public void UpdateDistance_ShouldCalculateDistanceInMeters()
    {
        // Arrange
        var ridePosition = new Position(33.8121, -117.9190);
        var ride = new RideTime("1", "Test Ride", 30, 10, ridePosition, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        var userPosition = new Position(33.8122, -117.9191); // Very close

        // Act
        vm.UpdateDistance(userPosition);

        // Assert
        vm.DistanceMeters.ShouldNotBeNull();
        vm.DistanceText.ShouldContain("m"); // Should be in meters since close distance
    }

    [Fact]
    public void UpdateDistance_ShouldShowKilometers_WhenFarAway()
    {
        // Arrange
        var ridePosition = new Position(33.8121, -117.9190);
        var ride = new RideTime("1", "Test Ride", 30, 10, ridePosition, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        var userPosition = new Position(34.0000, -118.0000); // Far away

        // Act
        vm.UpdateDistance(userPosition);

        // Assert
        vm.DistanceMeters.ShouldNotBeNull();
        vm.DistanceText.ShouldContain("km");
    }

    [Fact]
    public void UpdateDistance_WithNoRidePosition_ShouldNotUpdate()
    {
        // Arrange
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, localize, humanizer, services);

        var userPosition = new Position(33.8122, -117.9191);

        // Act
        vm.UpdateDistance(userPosition);

        // Assert
        vm.DistanceMeters.ShouldBeNull();
        vm.DistanceText.ShouldBe("Unknown");
    }
}