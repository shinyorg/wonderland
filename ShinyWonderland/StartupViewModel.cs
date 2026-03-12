namespace ShinyWonderland;

[ShellMap<StartupPage>]
public partial class StartupViewModel(
    CoreServices services,
    ILogger<StartupViewModel> logger,
    IGeofenceManager geofenceManager
) : ObservableObject, IPageLifecycleAware
{
    public async void OnAppearing()
    {
        await services.Notifications.RequestAccess();
        await this.TryGps();
        await this.TryGeofencing();

        await services.Navigator.SwitchShell(new AppShell(services.Localized));
    }

    public void OnDisappearing()
    {
    }
    
    
    async Task TryGps()
    {
        try
        {
            var access = await services.Gps.RequestAccess(GpsRequest.Realtime(true));
            
            // only check GPS if background is running and user has granted permissions
            if (access == AccessState.Available && services.Gps.CurrentListener == null)
            {
                var start = await services.IsUserWithinPark();
                if (start)
                    await services.Gps.StartListener(GpsRequest.Realtime(true));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start GPS");
        }
    }


    const string GEOFENCE_ID = "ThemePark";
    async Task TryGeofencing()
    {
        try
        {
            var access = await geofenceManager.RequestAccess();
            if (access == AccessState.Available)
            {
                var regions = geofenceManager.GetMonitorRegions();
                if (!regions.Any(x => x.Identifier.Equals(GEOFENCE_ID, StringComparison.InvariantCultureIgnoreCase)))
                {
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
            logger.LogWarning(ex, "Error with geofencing");
        }
    }
}