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

        gpsDelegate = new MyGpsDelegate(
            new AppSettings(),
            localized,
            new ILoggerImposter<MyGpsDelegate>().Instance(),
            parkOptions,
            new IGpsManagerImposter().Instance(),
            new INotificationManagerImposter().Instance(),
            new TestMediator()
        );
    }

    [Test]
    public async Task Constructor_ShouldNotThrow()
    {
        await Assert.That(gpsDelegate).IsNotNull();
    }

    [Test]
    public async Task MinimumDistance_ShouldBeNullBeforeFirstReading()
    {
        await Assert.That(gpsDelegate.MinimumDistance).IsNull();
    }

    [Test]
    public async Task MinimumTime_ShouldBeNullBeforeFirstReading()
    {
        await Assert.That(gpsDelegate.MinimumTime).IsNull();
    }
}
