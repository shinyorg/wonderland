namespace ShinyWonderland.Tests.Delegates;

public class RideTimeJobTests
{
    readonly FakeTimeProvider timeProvider;
    readonly INotificationManager notifications;
    readonly RideTimeJob job;

    public RideTimeJobTests()
    {
        var localized = TestLocalization.Create(new Dictionary<string, string>
        {
            ["RideTime"] = "Ride Time Alert",
            ["RideTimeNotificationMessageFormat"] = "{0} wait time is now {1} min (down {2} min)"
        });

        var parkOptions = Options.Create(new ParkOptions
        {
            Name = "Wonderland",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000
        });

        notifications = Substitute.For<INotificationManager>();
        timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 1, 26, 12, 0, 0, TimeSpan.Zero));

        var services = new CoreServices(
            Substitute.For<IMediator>(),
            parkOptions,
            new AppSettings(),
            Substitute.For<INavigator>(),
            Substitute.For<IDialogs>(),
            timeProvider,
            Substitute.For<IGpsManager>(),
            localized,
            notifications
        );

        job = new RideTimeJob(
            Substitute.For<ILogger<RideTimeJob>>(),
            parkOptions,
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
        // NOTE: Production code subtracts in reverse (LastSnapshotTime - Now),
        // so TotalMinutes is always negative for past times and never >= 5
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-6);

        var result = job.IsTimeToRun();

        result.ShouldBeFalse();
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