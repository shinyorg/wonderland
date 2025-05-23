namespace ShinyWonderland.Services;


public partial class AppSettings : ObservableObject
{
    [ObservableProperty] bool enableNotifications = true;
    [ObservableProperty] bool showOpenOnly = true;
    [ObservableProperty] bool showTimedOnly = true;
    [ObservableProperty] RideOrder ordering = RideOrder.Name;
}

public enum RideOrder
{
    Name,
    WaitTime
}