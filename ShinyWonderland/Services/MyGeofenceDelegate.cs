using Shiny.Notifications;

namespace ShinyWonderland.Services;

public class MyGeofenceDelegate(
    ILogger<MyGeofenceDelegate> logger,
    AppSettings appSettings,
    INotificationManager notifications,
    IOptions<ParkOptions> parkOptions
) : IGeofenceDelegate
{
    public async Task OnStatusChanged(GeofenceState newStatus, GeofenceRegion region)
    {
        logger.LogInformation("Geofence Hit");
        if (newStatus != GeofenceState.Entered)
            return;
        
        if (appSettings.EnableGeofenceReminder)
        {
            await notifications.Send(
                "Wonderland Reminder",
                "Don't forget to turn on the app for real time ride notifications and set your parking"
            );
        }
    }
}