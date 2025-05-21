using ShinyWonderland.Delegates;

namespace ShinyWonderland;

public partial class SettingsViewModel(AppSettings appSettings) : ObservableObject
{
    public string[] Sorts { get; } = ["Name", "Wait Time"];
    [ObservableProperty] public partial int SortByIndex { get; set; }
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; } = true;
}
