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
            Substitute.For<IMediator>(),
            parkOptions,
            new AppSettings(),
            Substitute.For<INavigator>(),
            Substitute.For<IDialogs>(),
            new FakeTimeProvider(DateTimeOffset.UtcNow),
            Substitute.For<IGpsManager>(),
            localized,
            Substitute.For<INotificationManager>()
        );

        gpsDelegate = new MyGpsDelegate(
            Substitute.For<ILogger<MyGpsDelegate>>(),
            services
        );
    }

    [Fact]
    public void Constructor_ShouldSetMinimumDistance()
    {
        // Assert
        gpsDelegate.MinimumDistance.ShouldNotBeNull();
        gpsDelegate.MinimumDistance!.TotalMeters.ShouldBe(10);
    }

    [Fact]
    public void Constructor_ShouldSetMinimumTime()
    {
        // Assert
        gpsDelegate.MinimumTime.ShouldNotBeNull();
        gpsDelegate.MinimumTime!.Value.TotalSeconds.ShouldBe(10);
    }
}