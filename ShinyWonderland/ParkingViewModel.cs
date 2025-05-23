using Shiny.Locations;
using ShinyWonderland.Services;

namespace ShinyWonderland;


public partial class ParkingViewModel(
    IGpsManager gpsManager,
    AppSettings appSettings,
    INavigator navigator,
    IConfiguration config
) : ObservableObject
{
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CommandText))]
    Position? parkLocation;
    
    public string CommandText => this.ParkLocation == null 
        ? "Set Parking Location to Current Location" 
        : "Remove Parking Location";
    
    public Position CenterOfPark => new(
        config.GetValue<double>("Park:Latitude"), 
        config.GetValue<double>("Park:Longitude")
    );
    
    [RelayCommand]
    async Task ToggleSetLocation()
    {
        if (appSettings.ParkingLocation == null)
        {
            var result = await gpsManager.RequestAccess(GpsRequest.Realtime(true));
            if (result is AccessState.Restricted or AccessState.Available)
            {
                var reading = await gpsManager.GetCurrentPosition().ToTask();
                appSettings.ParkingLocation = reading.Position;
                this.ParkLocation = reading.Position;
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
}