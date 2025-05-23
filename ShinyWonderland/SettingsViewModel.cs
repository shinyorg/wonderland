using System.ComponentModel;
using ShinyWonderland.Delegates;
using ShinyWonderland.Services;

namespace ShinyWonderland;


public partial class SettingsViewModel(AppSettings appSettings) : ObservableObject, INavigatedAware
{
    public string[] Sorts { get; } = ["Name", "Wait Time"];
    [ObservableProperty] public partial int SortByIndex { get; set; }
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; } = true;
    [ObservableProperty] public partial bool EnableNotifications { get; set; } = true;

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

    public void OnNavigatedTo()
    {
        this.EnableNotifications = appSettings.EnableNotifications;
        this.ShowOpenOnly = appSettings.ShowOpenOnly;
        this.SortByIndex = appSettings.Ordering switch
        {
            RideOrder.Name => 0,
            RideOrder.WaitTime => 1,
        };
    }
}
