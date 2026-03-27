namespace ShinyWonderland;

[ShellMap<StartupPage>(registerRoute: false)]
public partial class StartupViewModel(
    CoreServices services,
    ILogger<StartupViewModel> logger
) : ObservableObject
{

    public async void OnLoad()
    {
        try
        {
            await services.Notifications.RequestAccess();

            var main = Shell.Current.Items.FirstOrDefault(x => x.Route == "main");
            if (main != null)
                Shell.Current.CurrentItem = main;

            // GPS RequestAccess locks Shell navigation when it resolves without
            // a popup, so run it after we've already navigated away
            _ = this.TryGps();
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
                // var start = await services.IsUserWithinPark();
                // if (start)
                // {
                //     await services.Gps.StartListener(GpsRequest.Realtime(true));
                //
                // }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start GPS");
            await services.Dialogs.Alert("GPS Error", "Unable to start GPS tracking. " + ex);
        }
    }
}