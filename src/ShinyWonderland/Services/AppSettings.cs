namespace ShinyWonderland.Services;


[Singleton]
public partial class AppSettings : ObservableObject
{
#if IOS
    [Bind] public partial bool IsHeyWonderlandEnabled { get; set; }
#endif    
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
    [Bind] public partial string? VoiceId { get; set; }
    
    [Bind(Default = 100)] 
    public partial int SpeechRatePercent { get; set; }
    
    [Bind(Default = 100)] 
    public partial int PitchPercent { get; set; }
}

public enum RideOrder
{
    Name,
    WaitTime,
    PaidWaitTime,
    Distance
}