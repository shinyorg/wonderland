namespace ShinyWonderland.Tests.ViewModels;

public class RideTimesViewModelTests
{
    readonly ViewModelServices services;
    readonly ILogger<RideTimesViewModel> logger;
    readonly Humanizer humanizer;
    readonly StringsLocalized localize;

    public RideTimesViewModelTests()
    {
        logger = new ILoggerImposter<RideTimesViewModel>().Instance();
        localize = TestLocalization.Create(new Dictionary<string, string>
        {
            ["Error"] = "Error",
            ["GeneralError"] = "A general error occurred",
            ["Ok"] = "OK",
            ["UnknownDistance"] = "Unknown",
            ["Never"] = "Never",
            ["Second"] = "second",
            ["Seconds"] = "seconds",
            ["Minute"] = "minute",
            ["Minutes"] = "minutes",
            ["Hour"] = "hour",
            ["Hours"] = "hours"
        });

        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        humanizer = new Humanizer(timeProvider, localize);

        var parkOptions = new ParkOptions
        {
            Name = "Wonderland",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000,
            MapStartZoomDistanceMeters = 500
        };

        services = new ViewModelServices(
            new TestMediator(),
            Options.Create(parkOptions),
            new AppSettings(),
            new TestNavigator(),
            new IDialogsImposter().Instance(),
            timeProvider,
            new IGpsManagerImposter().Instance(),
            localize,
            new INotificationManagerImposter().Instance(),
            NullLoggerFactory.Instance
        );
    }

    RideTimesViewModel CreateViewModel() => new(
        services,
        logger,
        humanizer
    );

    [Test]
    public async Task Title_ShouldReturnParkName()
    {
        var vm = CreateViewModel();
        await Assert.That(vm.Title).IsEqualTo("Wonderland");
    }

    [Test]
    public async Task Localize_ShouldReturnInjectedLocalize()
    {
        var vm = CreateViewModel();
        await Assert.That(vm.Localize).IsEqualTo(localize);
    }

    [Test]
    public async Task Rides_ShouldStartEmpty()
    {
        var vm = CreateViewModel();
        await Assert.That(vm.Rides).IsNotNull();
        await Assert.That(vm.Rides.Count).IsEqualTo(0);
    }

    [Test]
    public async Task IsBusy_ShouldStartFalse()
    {
        var vm = CreateViewModel();
        await Assert.That(vm.IsBusy).IsFalse();
    }

    [Test]
    public async Task Handle_ConnectivityChanged_ShouldUpdateInternetAvailability()
    {
        var vm = CreateViewModel();
        var context = new TestMediatorContext();

        await vm.Handle(new ConnectivityChanged(true), context, CancellationToken.None);

        await Assert.That(vm.IsInternetAvailable).IsTrue();
    }

    [Test]
    public void OnDisappearing_ShouldNotThrow()
    {
        var vm = CreateViewModel();
        vm.OnDisappearing();
    }
}

public class RideTimeViewModelTests
{
    readonly StringsLocalized localize;
    readonly Humanizer humanizer;
    readonly ViewModelServices services;
    readonly FakeTimeProvider timeProvider;

    public RideTimeViewModelTests()
    {
        localize = TestLocalization.Create(new Dictionary<string, string>
        {
            ["UnknownDistance"] = "Unknown",
            ["HistoryDialogTitle"] = "Add to History",
            ["AddRideHistoryQuestion"] = "Add {0} to history?",
            ["Never"] = "Never",
            ["Seconds"] = "seconds",
            ["Minutes"] = "minutes"
        });

        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        humanizer = new Humanizer(timeProvider, localize);

        var parkOptions = new ParkOptions
        {
            Name = "Test Park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000
        };

        services = new ViewModelServices(
            new TestMediator(),
            Options.Create(parkOptions),
            new AppSettings(),
            new TestNavigator(),
            new IDialogsImposter().Instance(),
            timeProvider,
            new IGpsManagerImposter().Instance(),
            localize,
            new INotificationManagerImposter().Instance(),
            NullLoggerFactory.Instance
        );
    }

    [Test]
    public async Task Name_ShouldReturnRideName()
    {
        var ride = new RideTime("1", "Space Mountain", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        await Assert.That(vm.Name).IsEqualTo("Space Mountain");
    }

    [Test]
    public async Task WaitTimeMinutes_ShouldReturnRideWaitTime()
    {
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        await Assert.That(vm.WaitTimeMinutes).IsEqualTo(30);
        await Assert.That(vm.PaidWaitTimeMinutes).IsEqualTo(10);
    }

    [Test]
    public async Task IsOpen_ShouldReflectRideStatus()
    {
        var openRide = new RideTime("1", "Open Ride", 30, 10, null, true, null);
        var closedRide = new RideTime("2", "Closed Ride", null, null, null, false, null);

        var openVm = new RideTimeViewModel(openRide, humanizer, services);
        var closedVm = new RideTimeViewModel(closedRide, humanizer, services);

        await Assert.That(openVm.IsOpen).IsTrue();
        await Assert.That(openVm.IsClosed).IsFalse();
        await Assert.That(closedVm.IsOpen).IsFalse();
        await Assert.That(closedVm.IsClosed).IsTrue();
    }

    [Test]
    public async Task HasWaitTime_ShouldBeTrue_WhenWaitTimeHasValue()
    {
        var ride = new RideTime("1", "Test Ride", 30, null, null, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        await Assert.That(vm.HasWaitTime).IsTrue();
        await Assert.That(vm.HasPaidWaitTime).IsFalse();
    }

    [Test]
    public async Task HasLastRide_ShouldBeTrue_EvenWhenNeverRidden()
    {
        // NOTE: HasLastRide checks LastRidden != null, but TimeAgo(null) returns "Never" (non-null)
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        await Assert.That(vm.HasLastRide).IsTrue();
        await Assert.That(vm.LastRidden).IsEqualTo("Never");
    }

    [Test]
    public async Task HasLastRide_ShouldBeTrue_WhenPreviouslyRidden()
    {
        var lastRidden = DateTimeOffset.UtcNow.AddHours(-1);
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, lastRidden);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        await Assert.That(vm.HasLastRide).IsTrue();
        await Assert.That(vm.LastRidden).IsNotNull();
    }

    [Test]
    public async Task DistanceText_ShouldDefaultToUnknown()
    {
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        await Assert.That(vm.DistanceText).IsEqualTo("Unknown");
        await Assert.That(vm.DistanceMeters).IsNull();
    }

    [Test]
    public async Task UpdateDistance_ShouldCalculateDistanceInMeters()
    {
        var ridePosition = new Position(33.8121, -117.9190);
        var ride = new RideTime("1", "Test Ride", 30, 10, ridePosition, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        var userPosition = new Position(33.8122, -117.9191); // Very close

        vm.UpdateDistance(userPosition);

        await Assert.That(vm.DistanceMeters).IsNotNull();
        await Assert.That(vm.DistanceText).Contains("m");
    }

    [Test]
    public async Task UpdateDistance_ShouldShowKilometers_WhenFarAway()
    {
        var ridePosition = new Position(33.8121, -117.9190);
        var ride = new RideTime("1", "Test Ride", 30, 10, ridePosition, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        var userPosition = new Position(34.0000, -118.0000); // Far away

        vm.UpdateDistance(userPosition);

        await Assert.That(vm.DistanceMeters).IsNotNull();
        await Assert.That(vm.DistanceText).Contains("km");
    }

    [Test]
    public async Task UpdateDistance_WithNoRidePosition_ShouldNotUpdate()
    {
        var ride = new RideTime("1", "Test Ride", 30, 10, null, true, null);
        var vm = new RideTimeViewModel(ride, humanizer, services);

        var userPosition = new Position(33.8122, -117.9191);

        vm.UpdateDistance(userPosition);

        await Assert.That(vm.DistanceMeters).IsNull();
        await Assert.That(vm.DistanceText).IsEqualTo("Unknown");
    }
}
