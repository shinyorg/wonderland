namespace ShinyWonderland.Delegates;


public partial class AppSettings : ObservableObject
{
    [ObservableProperty] bool enableNotifications = true;
    [ObservableProperty] bool showOpenOnly;
    // selected sort
}