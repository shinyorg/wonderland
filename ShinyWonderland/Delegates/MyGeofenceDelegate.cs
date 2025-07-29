using Shiny.Notifications;

namespace ShinyWonderland.Delegates;

public class MyGeofenceDelegate(
    ILogger<MyGeofenceDelegate> logger,
    AppSettings appSettings,
    MyGeofenceDelegateLocalized localized,
    IOptions<ParkOptions> parkOptions,
    INotificationManager notifications
) : IGeofenceDelegate
{
    public async Task OnStatusChanged(GeofenceState newStatus, GeofenceRegion region)
    {
        logger.LogInformation("Geofence Hit");

        switch (newStatus)
        {
            case GeofenceState.Entered:
                if (appSettings.EnableGeofenceNotifications)
                {
                    await notifications.Send(
                        $"{parkOptions.Value.Name} {localized.Reminder}",
                        localized.NotificationMessage
                    );
                }
                break;
            
            case GeofenceState.Exited:
                // TODO: consider for shutdown later
                break;
        }
    }
}