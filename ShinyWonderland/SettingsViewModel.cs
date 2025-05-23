using System.ComponentModel;
using ShinyWonderland.Services;

namespace ShinyWonderland;


public partial class SettingsViewModel(AppSettings appSettings) : ObservableObject
{
    public string[] Sorts { get; } = ["Name", "Wait Time"];
    [ObservableProperty] public partial int SortByIndex { get; set; } = appSettings.Ordering switch
    {
        RideOrder.Name => 0,
        RideOrder.WaitTime => 1,
    };
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; } = appSettings.ShowOpenOnly;
    [ObservableProperty] public partial bool EnableNotifications { get; set; } = appSettings.EnableNotifications;
    [ObservableProperty] public partial bool ShowTimedOnly { get; set; } = appSettings.ShowTimedOnly;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SortByIndex):
                appSettings.Ordering = this.SortByIndex switch
                {
                    0 => RideOrder.Name,
                    1 => RideOrder.WaitTime
                };
                break;
            
            case nameof(ShowOpenOnly):
                appSettings.ShowOpenOnly = this.ShowOpenOnly;
                break;
            
            case nameof(EnableNotifications):
                appSettings.EnableNotifications = this.EnableNotifications;
                break;
        }
        base.OnPropertyChanged(e);
    }
}
