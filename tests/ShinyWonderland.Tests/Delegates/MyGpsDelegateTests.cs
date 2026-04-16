using Shiny.Notifications;

namespace ShinyWonderland.Tests.Delegates;

public class MyGpsDelegateTests
{
    readonly MyGpsDelegate gpsDelegate;

    public MyGpsDelegateTests()
    {
        var localized = TestLocalization.Create(new Dictionary<string, string>
        {
            ["LeaveParkNotificationMessage"] = "You have left the park area"
        });

        var parkOptions = Options.Create(new ParkOptions
        {
            Name = "Wonderland",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000
        });

        var services = new CoreServices(
            new TestMediator(),
            parkOptions,
            new AppSettings(),
            new TestNavigator(),
            new IDialogsImposter().Instance(),
            new FakeTimeProvider(DateTimeOffset.UtcNow),
            new IGpsManagerImposter().Instance(),
            localized,
            new INotificationManagerImposter().Instance()
        );

        gpsDelegate = new MyGpsDelegate(
            new ILoggerImposter<MyGpsDelegate>().Instance(),
            services
        );
    }

    [Test]
    public async Task Constructor_ShouldSetMinimumDistance()
    {
        await Assert.That(gpsDelegate.MinimumDistance).IsNotNull();
        await Assert.That(gpsDelegate.MinimumDistance!.TotalMeters).IsEqualTo(10);
    }

    [Test]
    public async Task Constructor_ShouldSetMinimumTime()
    {
        await Assert.That(gpsDelegate.MinimumTime).IsNotNull();
        await Assert.That(gpsDelegate.MinimumTime!.Value.TotalSeconds).IsEqualTo(10);
    }
}
