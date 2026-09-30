namespace ShinyWonderland.Features;


[ShellMap<SettingsPage>(registerRoute: false)]
public partial class SettingsViewModel : ObservableObject
{
    readonly AppSettings appSettings;
    readonly IAppInfo appInfo;

    public SettingsViewModel(AppSettings appSettings, IAppInfo appInfo)
    {
        this.appSettings = appSettings;
        this.appInfo = appInfo;

        this.Ordering = appSettings.Ordering;
        this.ShowOpenOnly = appSettings.ShowOpenOnly;
        this.EnableTimeRideNotifications = appSettings.EnableTimeRideNotifications;
        this.EnableDrinkNotifications = appSettings.EnableDrinkNotifications;
        this.EnableMealNotifications = appSettings.EnableMealNotifications;
        this.ShowTimedOnly = appSettings.ShowTimedOnly;
        this.EnableGeofenceNotifications = appSettings.EnableGeofenceNotifications;
    }

    public string AppVersion => this.appInfo.VersionString;

    [ObservableProperty] public partial RideOrder Ordering { get; set; }
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; }
    [ObservableProperty] public partial bool EnableTimeRideNotifications { get; set; }
    [ObservableProperty] public partial bool EnableDrinkNotifications { get; set; }
    [ObservableProperty] public partial bool EnableMealNotifications { get; set; }
    [ObservableProperty] public partial bool ShowTimedOnly { get; set; }
    [ObservableProperty] public partial bool EnableGeofenceNotifications { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Ordering):
                this.appSettings.Ordering = this.Ordering;
                break;

            case nameof(ShowOpenOnly):
                this.appSettings.ShowOpenOnly = this.ShowOpenOnly;
                break;

            case nameof(ShowTimedOnly):
                this.appSettings.ShowTimedOnly = this.ShowTimedOnly;
                break;

            case nameof(EnableGeofenceNotifications):
                this.appSettings.EnableGeofenceNotifications = this.EnableGeofenceNotifications;
                break;

            case nameof(EnableMealNotifications):
                this.appSettings.EnableMealNotifications = this.EnableMealNotifications;
                break;

            case nameof(EnableDrinkNotifications):
                this.appSettings.EnableDrinkNotifications = this.EnableDrinkNotifications;
                break;

            case nameof(EnableTimeRideNotifications):
                this.appSettings.EnableTimeRideNotifications = this.EnableTimeRideNotifications;
                break;
        }
        base.OnPropertyChanged(e);
    }
}
