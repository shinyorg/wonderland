using Shiny.Locations;

namespace ShinyWonderland.Services;


public partial class AppSettings : ObservableObject
{
    [ObservableProperty] bool enableTimeRideNotifications = true;
    [ObservableProperty] bool enableGeofenceNotifications = true;
    [ObservableProperty] bool enableDrinkNotifications = true;
    [ObservableProperty] bool enableMealNotifications = true;
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