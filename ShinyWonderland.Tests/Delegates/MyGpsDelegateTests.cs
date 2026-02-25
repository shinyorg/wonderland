namespace ShinyWonderland.Tests.Delegates;

public class MyGpsDelegateTests
{
    readonly ILogger<MyGpsDelegate> logger;
    readonly MyGpsDelegateLocalized localized;
    readonly IOptions<ParkOptions> parkOptions;
    readonly CoreServices services;
    readonly AppSettings appSettings;
    readonly IGpsManager gpsManager;
    readonly INotificationManager notifications;
    readonly IMediator mediator;
    readonly MyGpsDelegate gpsDelegate;
    readonly ParkOptions options;

    public MyGpsDelegateTests()
    {
        logger = Substitute.For<ILogger<MyGpsDelegate>>();
        localized = Substitute.For<MyGpsDelegateLocalized>();
        localized.NotificationMessage.Returns("You have left the park area");

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

        var navigator = Substitute.For<INavigator>();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        services = new CoreServices(
            mediator,
            parkOptions,
            appSettings,
            navigator,
            timeProvider,
            gpsManager,
            notifications
        );

        gpsDelegate = new MyGpsDelegate(
            logger,
            localized,
            parkOptions,
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