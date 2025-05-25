using System.ComponentModel;
using Shiny.Locations;
using ShinyWonderland.Services;

namespace ShinyWonderland;


public partial class SettingsViewModel(
    AppSettings appSettings,
    IGpsManager gpsManager,
    INavigator navigator
) : ObservableObject
{
    [RelayCommand] Task NavToHours() => navigator.NavigateTo("HoursPage");
    public string[] Sorts { get; } = ["Name", "Wait Time", "Paid Wait Time"];
    [ObservableProperty] public partial int SortByIndex { get; set; } = appSettings.Ordering switch
    {
        RideOrder.Name => 0,
        RideOrder.WaitTime => 1,
        RideOrder.PaidWaitTime => 2
    };
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; } = appSettings.ShowOpenOnly;
    [ObservableProperty] public partial bool EnableNotifications { get; set; } = appSettings.EnableNotifications;
    [ObservableProperty] public partial bool ShowTimedOnly { get; set; } = appSettings.ShowTimedOnly;
    [ObservableProperty] public partial bool EnableGeofenceReminder { get; set; } = appSettings.EnableGeofenceReminder;
    public bool IsNotificationActive => gpsManager.CurrentListener != null && appSettings.EnableNotifications; 

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
                };
                break;
            
            case nameof(ShowOpenOnly):
                appSettings.ShowOpenOnly = this.ShowOpenOnly;
                break;
            
            case nameof(ShowTimedOnly):
                appSettings.ShowTimedOnly = this.ShowTimedOnly;
                break;
            
            case nameof(EnableNotifications):
                appSettings.EnableNotifications = this.EnableNotifications;
                break;
        }
        base.OnPropertyChanged(e);
    }
}
