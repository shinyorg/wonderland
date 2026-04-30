namespace ShinyWonderland.Features.Parking;

public class ParkingModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddGeofencing<ParkingReminderGeofenceDelegate>();
    }

    public void Use(IPlatformApplication app)
    {
    }
}