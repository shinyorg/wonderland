using Shiny.Notifications;

namespace ShinyWonderland.Tests.Delegates;

public class MyGeofenceDelegateTests
{
    readonly AppSettings appSettings;
    readonly INotificationManagerImposter notifications;
    readonly MyGeofenceDelegate geofenceDelegate;

    public MyGeofenceDelegateTests()
    {
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

        notifications = new INotificationManagerImposter();

        geofenceDelegate = new MyGeofenceDelegate(
            new ILoggerImposter<MyGeofenceDelegate>().Instance(),
            appSettings,
            localized,
            parkOptions,
            notifications.Instance()
        );
    }

    [Test]
    public async Task OnStatusChanged_WhenEntered_AndNotificationsEnabled_ShouldSendNotification()
    {
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        await geofenceDelegate.OnStatusChanged(GeofenceState.Entered, region);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task OnStatusChanged_WhenEntered_AndNotificationsDisabled_ShouldNotSendNotification()
    {
        appSettings.EnableGeofenceNotifications = false;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        await geofenceDelegate.OnStatusChanged(GeofenceState.Entered, region);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task OnStatusChanged_WhenExited_ShouldNotSendNotification()
    {
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        await geofenceDelegate.OnStatusChanged(GeofenceState.Exited, region);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task OnStatusChanged_WithUnknownStatus_ShouldNotSendNotification()
    {
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        await geofenceDelegate.OnStatusChanged(GeofenceState.Unknown, region);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task OnStatusChanged_NotificationTitle_ShouldContainParkNameAndReminder()
    {
        appSettings.EnableGeofenceNotifications = true;
        var region = new GeofenceRegion("test", new Position(33.8121, -117.9190), Distance.FromMeters(1000));

        await geofenceDelegate.OnStatusChanged(GeofenceState.Entered, region);

        notifications
            .Send(Arg<Notification>.Is(n => n.Title!.Contains("Wonderland") && n.Title!.Contains("Reminder")))
            .Called(Count.Once());
    }
}
