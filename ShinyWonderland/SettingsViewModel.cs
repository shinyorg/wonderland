namespace ShinyWonderland;


[ShellMap<SettingsPage>(registerRoute: false)]
public partial class SettingsViewModel(AppSettings appSettings) : ObservableObject
{
    public string AppVersion => AssemblyInfo.ApplicationDisplayVersion;
    
    public string[] Sorts { get; } = ["Name", "Wait Time", "Paid Wait Time", "Distance"];
    [ObservableProperty] public partial int SortByIndex { get; set; } = appSettings.Ordering switch
    {
        RideOrder.Name => 0,
        RideOrder.WaitTime => 1,
        RideOrder.PaidWaitTime => 2,
        RideOrder.Distance => 3
    };
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
            case nameof(SortByIndex):
                appSettings.Ordering = this.SortByIndex switch
                {
                    0 => RideOrder.Name,
                    1 => RideOrder.WaitTime,
                    2 => RideOrder.PaidWaitTime,
                    3 => RideOrder.Distance
                };
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
