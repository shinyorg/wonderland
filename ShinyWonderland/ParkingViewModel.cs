

namespace ShinyWonderland;


public partial class ParkingViewModel(
    CoreServices services,
    ILogger<ParkingViewModel> logger
) : ObservableObject
{
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CommandText))]
    Position? parkLocation = services.AppSettings.ParkingLocation;

    [ObservableProperty] bool isBusy;
    
    public string CommandText => this.ParkLocation == null 
        ? "Set Parking Location to Current Location" 
        : "Remove Parking Location";

    public Position CenterOfPark => services.ParkOptions.Value.CenterOfPark;
    public int MapStartZoomDistanceMeters => services.ParkOptions.Value.MapStartZoomDistanceMeters;
    
    [RelayCommand]
    async Task ToggleSetLocation()
    {
        if (services.AppSettings.ParkingLocation == null)
        {
            var result = await services.Gps.RequestAccess(GpsRequest.Realtime(true));
            if (result is AccessState.Restricted or AccessState.Available)
            {
                await this.DoLocation();
            }
            else
            {
                var confirm = await services.Navigator.Confirm(
                    "Permission Denied",
                    "Do you wish to open app settings to change to the necessary permissions?",
                    "Yes", 
                    "No"
                );
                if (confirm)
                    AppInfo.ShowSettingsUI();
            }
        }
        else
        {
            var confirm = await services.Navigator.Confirm(
                "Reset?", 
                "Are you sure you want to reset the parking location?",
                "Yes",
                "No"
            );
            if (confirm)
            {
                services.AppSettings.ParkingLocation = null;
                this.ParkLocation = null;
            }
        }
    }


    async Task DoLocation()
    {
        try
        {
            this.IsBusy = true;
            var result = await services.TrySetParking(CancellationToken.None);

            if (result.IsWithinPark)
            {
                this.ParkLocation = result.Position!;
            }
            else
            {
                await services.Navigator.Alert(
                    "ERROR",
                    "You aren't close enough to the park to use the parking function",
                    "OK"
                );
            }
        }
        catch (Exception e)
        {
            await services.Navigator.Alert("ERROR", "Error retrieving current position", "OK");
            logger.LogError(e, "Error retrieving current position");
        }
        finally
        {
            this.IsBusy = false;
        }
    }
}