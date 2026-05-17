namespace ShinyWonderland.Services;


[Reflector]
public partial class AppSettings : ObservableObject
{
#if IOS
    [ObservableProperty] public partial bool IsHeyWonderlandEnabled { get; set; }
#endif    
    [ObservableProperty] public partial bool EnableTimeRideNotifications { get; set; } = true;
    [ObservableProperty] public partial bool EnableGeofenceNotifications { get; set; } = true;
    [ObservableProperty] public partial bool EnableDrinkNotifications { get; set; } = true;
    [ObservableProperty] public partial bool EnableMealNotifications { get; set; } = true;
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; } = true;
    [ObservableProperty] public partial bool ShowTimedOnly { get; set; } = true;
    [ObservableProperty] public partial RideOrder Ordering { get; set; } = RideOrder.Name;
    [ObservableProperty] public partial Position? ParkingLocation { get; set; }
    [ObservableProperty] public partial string? VoiceId { get; set; }
    [ObservableProperty] public partial int SpeechRatePercent { get; set; } = 100;
    [ObservableProperty] public partial int PitchPercent { get; set; } = 100;
}

public enum RideOrder
{
    Name,
    WaitTime,
    PaidWaitTime,
    Distance
}