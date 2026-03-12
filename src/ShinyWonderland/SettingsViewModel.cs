namespace ShinyWonderland;


[ShellMap<SettingsPage>(registerRoute: false)]
public partial class SettingsViewModel(AppSettings appSettings) : ObservableObject
{
    public string AppVersion => AssemblyInfo.ApplicationDisplayVersion;
    
    [ObservableProperty] public partial RideOrder Ordering { get; set; } = appSettings.Ordering;
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; } = appSettings.ShowOpenOnly;
    [ObservableProperty] public partial bool EnableTimeRideNotifications { get; set; } = appSettings.EnableTimeRideNotifications;
    [ObservableProperty] public partial bool EnableDrinkNotifications { get; set; } = appSettings.EnableDrinkNotifications;
    [ObservableProperty] public partial bool EnableMealNotifications { get; set; } = appSettings.EnableMealNotifications;
    [ObservableProperty] public partial bool ShowTimedOnly { get; set; } = appSettings.ShowTimedOnly;
    [ObservableProperty] public partial bool EnableGeofenceNotifications { get; set; } = appSettings.EnableGeofenceNotifications;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Ordering):
                appSettings.Ordering = this.Ordering;
                break;
            
            case nameof(ShowOpenOnly):
                appSettings.ShowOpenOnly = this.ShowOpenOnly;
                break;
            
            case nameof(ShowTimedOnly):
                appSettings.ShowTimedOnly = this.ShowTimedOnly;
                break;
            
            case nameof(EnableGeofenceNotifications):
                appSettings.EnableGeofenceNotifications = this.EnableGeofenceNotifications;
                break;
            
            case nameof(EnableMealNotifications):
                appSettings.EnableMealNotifications = this.EnableMealNotifications;
                break;
            
            case nameof(EnableDrinkNotifications):
                appSettings.EnableDrinkNotifications = this.EnableDrinkNotifications;
                break;
            
            case nameof(EnableTimeRideNotifications):
                appSettings.EnableTimeRideNotifications = this.EnableTimeRideNotifications;
                break;
        }
        base.OnPropertyChanged(e);
    }
}
