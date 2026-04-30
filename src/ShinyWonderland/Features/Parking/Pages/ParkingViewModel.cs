namespace ShinyWonderland.Features.Parking.Pages;


[ShellMap<ParkingPage>(registerRoute: false)]
public partial class ParkingViewModel(
    ViewModelServices services,
    IMediaPicker mediaPicker
) : BaseViewModel(services)
{
    const string PhotoFileName = "parked_photo.png";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsParked))]
    [NotifyPropertyChangedFor(nameof(CommandText))]
    Position? parkLocation;

    [ObservableProperty] bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParkedImage))]
    string? imageUri;

    public bool HasParkedImage => ImageUri != null;
    public bool IsParked => this.ParkLocation != null;
    public string CommandText => this.ParkLocation == null
        ? Localize.SetParking
        : Localize.RemoveParking;

    public Position CenterOfPark => services.ParkOptions.Value.CenterOfPark;
    public int MapStartZoomDistanceMeters => services.ParkOptions.Value.MapStartZoomDistanceMeters;

    static string PhotoPath => Path.Combine(FileSystem.AppDataDirectory, PhotoFileName);

    [RelayCommand]
    async Task ToggleSetLocation()
    {
        if (services.AppSettings.ParkingLocation == null)
        {
            var result = services.Gps.GetCurrentStatus(GpsRequest.Realtime(true));
            if (result is AccessState.Restricted or AccessState.Available)
            {
                await this.DoLocation();
            }
            else
            {
                var confirm = await services.Dialogs.Confirm(
                    Localize.PermissionDenied,
                    Localize.OpenSettings
                );
                if (confirm)
                    AppInfo.ShowSettingsUI();
            }
        }
        else
        {
            var confirm = await services.Dialogs.Confirm(
                Localize.Reset,
                Localize.ConfirmReset
            );
            if (confirm)
            {
                services.AppSettings.ParkingLocation = null;
                this.ParkLocation = null;
                DeletePhoto();
            }
        }
    }

    [RelayCommand]
    async Task TakePhoto()
    {
        try
        {
            var result = await mediaPicker.CapturePhotoAsync();
            if (result != null)
            {
                await using var sourceStream = await result.OpenReadAsync();
                await using var destStream = File.Create(PhotoPath);
                await sourceStream.CopyToAsync(destStream);
                this.ImageUri = PhotoPath;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to capture photo");
        }
    }

    void DeletePhoto()
    {
        try
        {
            if (File.Exists(PhotoPath))
                File.Delete(PhotoPath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete parking photo");
        }
        this.ImageUri = null;
    }

    async Task DoLocation()
    {
        try
        {
            this.IsBusy = true;
            var result = await services.TrySetParking(this.DeactivateToken);

            if (result.IsWithinPark)
            {
                this.ParkLocation = result.Position!;
            }
            else
            {
                await services.Dialogs.Alert(
                    Localize.Error,
                    Localize.NotCloseEnough
                );
            }
        }
        catch (Exception e)
        {
            await services.Dialogs.Alert(Localize.Error, Localize.ErrorRetrievingLocation);
            Logger.LogError(e, "Error retrieving current position");
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    public void OnAppearing()
    {
        ParkLocation = services.AppSettings.ParkingLocation;
        if (File.Exists(PhotoPath))
            this.ImageUri = PhotoPath;
    }

    public void OnDisappearing() {}
}
