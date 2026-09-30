namespace ShinyWonderland.Services;


[Singleton]
public partial class AppSettings : ObservableObject
{
    [Bind(Default = true)] 
    public partial bool EnableTimeRideNotifications { get; set; }
    
    [Bind(Default = true)] 
    public partial bool EnableGeofenceNotifications { get; set; }
    
    [Bind(Default = true)] 
    public partial bool EnableDrinkNotifications { get; set; }
    
    [Bind(Default = true)] 
    public partial bool EnableMealNotifications { get; set; } 
    
    [Bind(Default = true)] 
    public partial bool ShowOpenOnly { get; set; }

    [Bind(Default = true)] 
    public partial bool ShowTimedOnly { get; set; }
    
    [Bind(Default = RideOrder.Name)] 
    public partial RideOrder Ordering { get; set; }
    
    [Bind] public partial Position? ParkingLocation { get; set; }
}

public enum RideOrder
{
    Name,
    WaitTime,
    PaidWaitTime,
    Distance
}