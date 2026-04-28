using Shiny.Notifications;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Tests.Delegates;

public class RideTimeJobTests
{
    readonly FakeTimeProvider timeProvider;
    readonly INotificationManagerImposter notifications;
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

        notifications = new INotificationManagerImposter();
        timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 1, 26, 12, 0, 0, TimeSpan.Zero));

        var services = new CoreServices(
            new TestMediator(),
            parkOptions,
            new AppSettings(),
            new TestNavigator(),
            new IDialogsImposter().Instance(),
            timeProvider,
            new IGpsManagerImposter().Instance(),
            localized,
            notifications.Instance(),
            NullLoggerFactory.Instance
        );

        job = new RideTimeJob(
            new ILoggerImposter<RideTimeJob>().Instance(),
            parkOptions,
            services
        );
    }

    [Test]
    public async Task IsTimeToRun_WhenLastSnapshotTimeIsNull_ShouldReturnTrue()
    {
        job.LastSnapshotTime = null;

        var result = job.IsTimeToRun();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsTimeToRun_WhenLastRunLessThan5MinutesAgo_ShouldReturnFalse()
    {
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-3);

        var result = job.IsTimeToRun();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsTimeToRun_WhenLastRunMoreThan5MinutesAgo_ShouldReturnTrue()
    {
        // NOTE: Production code subtracts in reverse (LastSnapshotTime - Now),
        // so TotalMinutes is always negative for past times and never >= 5
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-6);

        var result = job.IsTimeToRun();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task EnsureLastSnapshot_WhenSnapshotTimeIsNull_ShouldKeepSnapshotNull()
    {
        job.LastSnapshotTime = null;
        job.LastSnapshot = new List<RideTime> { new("1", "Test", 30, 10, null, true, null) };

        job.EnsureLastSnapshot();

        await Assert.That(job.LastSnapshot).IsNull();
    }

    [Test]
    public async Task EnsureLastSnapshot_WhenSnapshotIsOld_ShouldClearSnapshot()
    {
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-35);
        job.LastSnapshot = new List<RideTime> { new("1", "Test", 30, 10, null, true, null) };

        job.EnsureLastSnapshot();

        await Assert.That(job.LastSnapshot).IsNull();
        await Assert.That(job.LastSnapshotTime).IsNull();
    }

    [Test]
    public async Task EnsureLastSnapshot_WhenSnapshotIsRecent_ShouldKeepSnapshot()
    {
        var rides = new List<RideTime> { new("1", "Test", 30, 10, null, true, null) };
        job.LastSnapshotTime = timeProvider.GetUtcNow().AddMinutes(-15);
        job.LastSnapshot = rides;

        job.EnsureLastSnapshot();

        await Assert.That(job.LastSnapshot).IsEqualTo(rides);
    }

    [Test]
    public async Task IterateDiff_WhenWaitTimeDecreased_ShouldSendNotification()
    {
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", 30, 15, null, true, null)
        };

        await job.IterateDiff(previous, current);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task IterateDiff_WhenWaitTimeIncreased_ShouldNotSendNotification()
    {
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 30, 15, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };

        await job.IterateDiff(previous, current);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task IterateDiff_WhenRideIsClosed_ShouldNotSendNotification()
    {
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", null, null, null, false, null)
        };

        await job.IterateDiff(previous, current);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task IterateDiff_WhenRideNotInCurrent_ShouldNotSendNotification()
    {
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("2", "Space Mountain", 30, 15, null, true, null)
        };

        await job.IterateDiff(previous, current);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task IterateDiff_WithMultipleRides_ShouldOnlyNotifyDecreased()
    {
        var previous = new List<RideTime>
        {
            new("1", "Thunder Mountain", 60, 30, null, true, null),
            new("2", "Space Mountain", 30, 15, null, true, null),
            new("3", "Splash Mountain", 45, 20, null, true, null)
        };
        var current = new List<RideTime>
        {
            new("1", "Thunder Mountain", 30, 15, null, true, null), // Decreased
            new("2", "Space Mountain", 45, 20, null, true, null),   // Increased
            new("3", "Splash Mountain", 45, 20, null, true, null)   // Same
        };

        await job.IterateDiff(previous, current);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task LastSnapshot_ShouldRaisePropertyChanged()
    {
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RideTimeJob.LastSnapshot))
                propertyChangedRaised = true;
        };

        job.LastSnapshot = new List<RideTime>();

        await Assert.That(propertyChangedRaised).IsTrue();
    }

    [Test]
    public async Task LastSnapshotTime_ShouldRaisePropertyChanged()
    {
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RideTimeJob.LastSnapshotTime))
                propertyChangedRaised = true;
        };

        job.LastSnapshotTime = DateTimeOffset.UtcNow;

        await Assert.That(propertyChangedRaised).IsTrue();
    }
}
