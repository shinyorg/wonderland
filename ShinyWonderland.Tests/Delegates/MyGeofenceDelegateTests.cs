namespace ShinyWonderland.Tests.Delegates;

public class MyGeofenceDelegateTests
{
    readonly ILogger<MyGeofenceDelegate> logger;
    readonly AppSettings appSettings;
    readonly MyGeofenceDelegateLocalized localized;
    readonly IOptions<ParkOptions> parkOptions;
    readonly INotificationManager notifications;
    readonly MyGeofenceDelegate geofenceDelegate;

    public MyGeofenceDelegateTests()
    {
        logger = Substitute.For<ILogger<MyGeofenceDelegate>>();
        appSettings = new AppSettings();
        localized = Substitute.For<MyGeofenceDelegateLocalized>();
        localized.Reminder.Returns("Reminder");
        localized.NotificationMessage.Returns("Welcome to the park!");

        var options = new ParkOptions
        {
            Name = "Wonderland",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000
        };
        parkOptions = Options.Create(options);

        notifications = Substitute.For<INotificationManager>();

        geofenceDelegate = new MyGeofenceDelegate(
            logger,
            appSettings,
            localized,
            parkOptions,
            notifications
        );
    }

    [Fact]
    public async Task OnStatusChanged_WhenEntered_AndNotificationsEnabled_ShouldSendNotification()
    {
        // Arrange
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        // Act
        await geofenceDelegate.OnStatusChanged(GeofenceState.Entered, region);

        // Assert
        await notifications.Received(1).Send(
            Arg.Is<string>(t => t.Contains("Wonderland") && t.Contains("Reminder")),
            Arg.Is<string>(m => m == "Welcome to the park!")
        );
    }

    [Fact]
    public async Task OnStatusChanged_WhenEntered_AndNotificationsDisabled_ShouldNotSendNotification()
    {
        // Arrange
        appSettings.EnableGeofenceNotifications = false;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        // Act
        await geofenceDelegate.OnStatusChanged(GeofenceState.Entered, region);

        // Assert
        await notifications.DidNotReceive().Send(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnStatusChanged_WhenExited_ShouldNotSendNotification()
    {
        // Arrange
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        // Act
        await geofenceDelegate.OnStatusChanged(GeofenceState.Exited, region);

        // Assert
        await notifications.DidNotReceive().Send(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnStatusChanged_WithUnknownStatus_ShouldNotSendNotification()
    {
        // Arrange
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        // Act
        await geofenceDelegate.OnStatusChanged(GeofenceState.Unknown, region);

        // Assert
        await notifications.DidNotReceive().Send(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnStatusChanged_NotificationTitle_ShouldContainParkNameAndReminder()
    {
        // Arrange
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));
        string? capturedTitle = null;

        notifications.Send(Arg.Do<string>(t => capturedTitle = t), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        await geofenceDelegate.OnStatusChanged(GeofenceState.Entered, region);

        // Assert
        capturedTitle.ShouldNotBeNull();
        capturedTitle.ShouldContain("Wonderland");
        capturedTitle.ShouldContain("Reminder");
    }
}