namespace ShinyWonderland.Tests.Delegates;

public class RideTimeJobTests
{
    readonly ILogger<RideTimeJob> logger;
    readonly IOptions<ParkOptions> parkOptions;
    readonly RideTimeJobLocalized localized;
    readonly CoreServices services;
    readonly AppSettings appSettings;
    readonly IGpsManager gpsManager;
    readonly INotificationManager notifications;
    readonly IMediator mediator;
    readonly FakeTimeProvider timeProvider;
    readonly RideTimeJob job;
    readonly ParkOptions options;

    public RideTimeJobTests()
    {
        logger = Substitute.For<ILogger<RideTimeJob>>();
        localized = Substitute.For<RideTimeJobLocalized>();
        localized.RideTime.Returns("Ride Time Alert");
        localized.NotificationMessageFormatFormat(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<int?>())
            .Returns(x => $"{x[0]} wait time is now {x[1]} min (down {x[2]} min)");

        options = new ParkOptions
        {
            Name = "Wonderland",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000
        };
        parkOptions = Options.Create(options);

        appSettings = new AppSettings();
        gpsManager = Substitute.For<IGpsManager>();
        notifications = Substitute.For<INotificationManager>();
        mediator = Substitute.For<IMediator>();
        timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 1, 26, 12, 0, 0, TimeSpan.Zero));

        var navigator = Substitute.For<INavigator>();

        services = new CoreServices(
            mediator,
            parkOptions,
            appSettings,
            navigator,
            timeProvider,
            gpsManager,
            notifications
        );

        job = new RideTimeJob(
            logger,
            parkOptions,
            localized,
            services
        );
    }

    [Fact]
    public void IsTimeToRun_WhenLastSnapshotTimeIsNull_ShouldReturnTrue()
    {
        // Arrange
        job.LastSnapshotTime = null;

        // Act
        var result = job.IsTimeToRun();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsTimeToRun_WhenLastRunLessThan5MinutesAgo_ShouldReturnFalse()
    {
        // Arrange
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-3);

        // Act
        var result = job.IsTimeToRun();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsTimeToRun_WhenLastRunMoreThan5MinutesAgo_ShouldReturnTrue()
    {
        // Arrange
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-6);

        // Act
        var result = job.IsTimeToRun();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EnsureLastSnapshot_WhenSnapshotTimeIsNull_ShouldKeepSnapshotNull()
    {
        // Arrange
        job.LastSnapshotTime = null;
        job.LastSnapshot = new List<RideTime> { new("1", "Test", 30, 10, null, true, null) };

        // Act
        job.EnsureLastSnapshot();

        // Assert
        job.LastSnapshot.ShouldBeNull();
    }

    [Fact]
    public void EnsureLastSnapshot_WhenSnapshotIsOld_ShouldClearSnapshot()
    {
        // Arrange
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-35);
        job.LastSnapshot = new List<RideTime> { new("1", "Test", 30, 10, null, true, null) };

        // Act
        job.EnsureLastSnapshot();

        // Assert
        job.LastSnapshot.ShouldBeNull();
        job.LastSnapshotTime.ShouldBeNull();
    }

    [Fact]
    public void EnsureLastSnapshot_WhenSnapshotIsRecent_ShouldKeepSnapshot()
    {
        // Arrange
        var rides = new List<RideTime> { new("1", "Test", 30, 10, null, true, null) };
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-15);
        job.LastSnapshot = rides;

        // Act
        job.EnsureLastSnapshot();

        // Assert
        job.LastSnapshot.ShouldBe(rides);
    }

    [Fact]
    public async Task IterateDiff_WhenWaitTimeDecreased_ShouldSendNotification()
    {
        // Arrange
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", 30, 15, null, true, null)
        };

        // Act
        await job.IterateDiff(previous, current);

        // Assert
        await notifications.Received(1).Send(Arg.Any<Notification>());
    }

    [Fact]
    public async Task IterateDiff_WhenWaitTimeIncreased_ShouldNotSendNotification()
    {
        // Arrange
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 30, 15, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };

        // Act
        await job.IterateDiff(previous, current);

        // Assert
        await notifications.DidNotReceive().Send(Arg.Any<Notification>());
    }

    [Fact]
    public async Task IterateDiff_WhenRideIsClosed_ShouldNotSendNotification()
    {
        // Arrange
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", null, null, null, false, null)
        };

        // Act
        await job.IterateDiff(previous, current);

        // Assert
        await notifications.DidNotReceive().Send(Arg.Any<Notification>());
    }

    [Fact]
    public async Task IterateDiff_WhenRideNotInCurrent_ShouldNotSendNotification()
    {
        // Arrange
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("2", "Space Mountain", 30, 15, null, true, null)
        };

        // Act
        await job.IterateDiff(previous, current);

        // Assert
        await notifications.DidNotReceive().Send(Arg.Any<Notification>());
    }

    [Fact]
    public async Task IterateDiff_WithMultipleRides_ShouldOnlyNotifyDecreased()
    {
        // Arrange
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null),
            new("2", "Space Mountain", 30, 15, null, true, null),
            new("3", "Splash Mountain", 45, 20, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", 30, 15, null, true, null), // Decreased
            new("2", "Space Mountain", 45, 20, null, true, null), // Increased
            new("3", "Splash Mountain", 45, 20, null, true, null)  // Same
        };

        // Act
        await job.IterateDiff(previous, current);

        // Assert
        await notifications.Received(1).Send(Arg.Any<Notification>());
    }

    [Fact]
    public void LastSnapshot_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RideTimeJob.LastSnapshot))
                propertyChangedRaised = true;
        };

        // Act
        job.LastSnapshot = new List<RideTime>();

        // Assert
        propertyChangedRaised.ShouldBeTrue();
    }

    [Fact]
    public void LastSnapshotTime_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RideTimeJob.LastSnapshotTime))
                propertyChangedRaised = true;
        };

        // Act
        job.LastSnapshotTime = DateTimeOffset.UtcNow;

        // Assert
        propertyChangedRaised.ShouldBeTrue();
    }
}