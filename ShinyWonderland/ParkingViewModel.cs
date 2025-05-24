

namespace ShinyWonderland;


public partial class ParkingViewModel(
    IGpsManager gpsManager,
    AppSettings appSettings,
    INavigator navigator,
    IOptions<ParkOptions> parkOptions,
    ILogger<ParkingViewModel> logger
) : ObservableObject
{
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CommandText))]
    Position? parkLocation = appSettings.ParkingLocation;

    [ObservableProperty] bool isBusy;
    
    public string CommandText => this.ParkLocation == null 
        ? "Set Parking Location to Current Location" 
        : "Remove Parking Location";

    public Position CenterOfPark => parkOptions.Value.CenterOfPark;
    
    [RelayCommand]
    async Task ToggleSetLocation()
    {
        if (appSettings.ParkingLocation == null)
        {
            var result = await gpsManager.RequestAccess(GpsRequest.Realtime(true));
            if (result is AccessState.Restricted or AccessState.Available)
            {
                await this.DoLocation();
            }
            else
            {
                var confirm = await navigator.Confirm(
                    "Permission Denied",
                    "Do you wish to open app settings to change to the necessary permissions?"
                );
                if (confirm)
                    AppInfo.ShowSettingsUI();
            }
        }
        else
        {
            var confirm = await navigator.Confirm(
                "Reset?", 
                "Are you sure you want to reset the parking location?"
            );
            if (confirm)
            {
                appSettings.ParkingLocation = null;
                this.ParkLocation = null;
            }
        }
    }


    async Task DoLocation()
    {
        try
        {
            this.IsBusy = true;
            var reading = await gpsManager.GetCurrentPosition().ToTask();

            var dist = parkOptions.Value.CenterOfPark.GetDistanceTo(reading.Position);
            if (dist.TotalKilometers > 2)
            {
                await navigator.Alert(
                    "ERROR",
                    "You aren't close enough to the park to use the parking function"
                );
            }
            else
            {
                appSettings.ParkingLocation = reading.Position;
                this.ParkLocation = reading.Position;
            }
        }
        catch (Exception e)
        {
            await navigator.Alert("ERROR", "Error retrieving current position");
            logger.LogError(e, "Error retrieving current position");
        }
        finally
        {
            this.IsBusy = false;
        }
    }
}