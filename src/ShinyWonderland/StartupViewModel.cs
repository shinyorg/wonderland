namespace ShinyWonderland;

[ShellMap<StartupPage>(registerRoute: false)]
public partial class StartupViewModel(
    CoreServices services,
    ILogger<StartupViewModel> logger,
    IGeofenceManager geofenceManager
) : ObservableObject
{
    const string GEOFENCE_ID = "ThemePark";

    public async void OnLoad()
    {
        try
        {
            await services.Notifications.RequestAccess();
            await this.TryGps();
            await services.Navigator.NavigateTo("//main/ridetimes");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to navigate to MainPage");
            await services.Dialogs.Alert("Startup Error", "An error occurred during startup. " + ex);
        }
    }


    static bool gpsChecked = false;
    async Task TryGps()
    {
        if (gpsChecked)
            return;
        
        try
        {
            gpsChecked = true;
            var access = await services.Gps.RequestAccess(GpsRequest.Realtime(true));
            
            // only check GPS if background is running and user has granted permissions
            if (access == AccessState.Available && services.Gps.CurrentListener == null)
            {
                var start = await services.IsUserWithinPark();
                if (start)
                {
                    await services.Gps.StartListener(GpsRequest.Realtime(true));

                    await geofenceManager.StopAllMonitoring();
                    await geofenceManager.StartMonitoring(new GeofenceRegion(
                        GEOFENCE_ID,
                        services.ParkOptions.Value.CenterOfPark,
                        services.ParkOptions.Value.NotificationDistance
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start GPS");
            await services.Dialogs.Alert("GPS Error", "Unable to start GPS tracking. " + ex);
        }
    }
}