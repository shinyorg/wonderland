using Shiny.Locations;

namespace ShinyWonderland.Services;


public partial class AppSettings : ObservableObject
{
    [ObservableProperty] bool enableNotifications = true;
    [ObservableProperty] bool enableGeofenceReminder = true;
    [ObservableProperty] bool showOpenOnly = true;
    [ObservableProperty] bool showTimedOnly = true;
    [ObservableProperty] RideOrder ordering = RideOrder.Name;
    [ObservableProperty] Position? parkingLocation;
}

public enum RideOrder
{
    Name,
    WaitTime,
    PaidWaitTime,
    Distance
}