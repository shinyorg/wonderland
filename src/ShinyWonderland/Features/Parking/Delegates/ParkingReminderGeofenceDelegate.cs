using Shiny.Notifications;

namespace ShinyWonderland.Features.Parking.Delegates;

public class ParkingReminderGeofenceDelegate(
    ILogger<ParkingReminderGeofenceDelegate> logger,
    AppSettings appSettings,
    StringsLocalized localized,
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
                        localized.EnterParkNotificationMessage
                    );
                }
                break;
            
            case GeofenceState.Exited:
                // TODO: consider for shutdown later
                break;
        }
    }
}
