namespace ShinyWonderland.Delegates;


public partial class AppSettings : ObservableObject
{
    [ObservableProperty] bool enableNotifications = true;
    [ObservableProperty] bool showOpenOnly;
    [ObservableProperty] RideOrder ordering = RideOrder.Name;
}

public enum RideOrder
{
    Name,
    WaitTime
}