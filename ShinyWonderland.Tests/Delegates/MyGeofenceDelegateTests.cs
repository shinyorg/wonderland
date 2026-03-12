namespace ShinyWonderland.Tests.Delegates;

public class MyGeofenceDelegateTests
{
    readonly AppSettings appSettings;
    readonly INotificationManager notifications;
    readonly MyGeofenceDelegate geofenceDelegate;

    public MyGeofenceDelegateTests()
    {
        var logger = Substitute.For<ILogger<MyGeofenceDelegate>>();
        appSettings = new AppSettings();

        var localized = TestLocalization.Create(new Dictionary<string, string>
        {
            ["Reminder"] = "Reminder",
            ["EnterParkNotificationMessage"] = "Welcome to the park!"
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

        // Assert - Send(string, string) is an extension method; assert on the interface method Send(Notification)
        await notifications.Received(1).Send(Arg.Is<Notification>(n =>
            n.Title!.Contains("Wonderland") &&
            n.Title!.Contains("Reminder") &&
            n.Message == "Welcome to the park!"
        ));
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
        await notifications.DidNotReceive().Send(Arg.Any<Notification>());
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
        await notifications.DidNotReceive().Send(Arg.Any<Notification>());
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
        await notifications.DidNotReceive().Send(Arg.Any<Notification>());
    }

    [Fact]
    public async Task OnStatusChanged_NotificationTitle_ShouldContainParkNameAndReminder()
    {
        // Arrange
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));
        Notification? captured = null;

        notifications.Send(Arg.Do<Notification>(n => captured = n));

        // Act
        await geofenceDelegate.OnStatusChanged(GeofenceState.Entered, region);

        // Assert
        captured.ShouldNotBeNull();
        captured.Title!.ShouldContain("Wonderland");
        captured.Title!.ShouldContain("Reminder");
    }
}