namespace ShinyWonderland;


[ShellMap<ParkingPage>(registerRoute: false)]
public partial class ParkingViewModel(
    CoreServices services,
    IMediaPicker mediaPicker,
    ILogger<ParkingViewModel> logger
) : ObservableObject, IPageLifecycleAware
{
    const string PhotoFileName = "parked_photo.png";
    
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsParked))]
    [NotifyPropertyChangedFor(nameof(CommandText))]
    Position? parkLocation;

    [ObservableProperty] bool isBusy;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParkedImage))]
    string imageUri;

    public bool HasParkedImage => ImageUri != null;
    public bool IsParked => this.ParkLocation != null;
    
    
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
                    "Do you wish to open app settings to change to the necessary permissions?"
                );
                if (confirm)
                    AppInfo.ShowSettingsUI();
            }
        }
        else
        {
            var confirm = await services.Navigator.Confirm(
                "Reset?", 
                "Are you sure you want to reset the parking location?"
            );
            if (confirm)
            {
                services.AppSettings.ParkingLocation = null;
                this.ParkLocation = null;
                
                // TODO: delete photo
            }
        }
    }


    [RelayCommand]
    async Task TogglePhotoZoom()
    {
        // TODO: zoom in or out on photo
    }

    [RelayCommand]
    async Task TakePhoto()
    {
        try
        {
            var result = await mediaPicker.CapturePhotoAsync();
            if (result != null)
            {
                // TODO: save photo to local storage
                // TODO: change imageUri to the saved photo path
            }
        }
        catch (Exception ex)
        {
            
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
                    "You aren't close enough to the park to use the parking function"
                );
            }
        }
        catch (Exception e)
        {
            await services.Navigator.Alert("ERROR", "Error retrieving current position");
            logger.LogError(e, "Error retrieving current position");
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    public void OnAppearing()
    {
        ParkLocation = services.AppSettings.ParkingLocation;
        // TODO: load imageUri from local storage if exists
    }

    public void OnDisappearing() {}
}