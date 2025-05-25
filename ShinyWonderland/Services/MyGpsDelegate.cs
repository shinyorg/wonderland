using Shiny.Notifications;

namespace ShinyWonderland.Services;


public class MyGpsDelegate : GpsDelegate
{
    readonly INotificationManager notifications;
    readonly IGpsManager gpsManager;
    readonly ParkOptions parkOptions;
    
    public MyGpsDelegate(
        ILogger<MyGpsDelegate> logger,
        IOptions<ParkOptions> parkOptions,
        INotificationManager notifications,
        IGpsManager gpsManager
    ) : base(logger)
    {
        this.MinimumDistance = Distance.FromMeters(10);
        this.MinimumTime = TimeSpan.FromSeconds(10);

        this.gpsManager = gpsManager;
        this.parkOptions = parkOptions.Value;
        this.notifications = notifications;
    }

    
    protected override async Task OnGpsReading(GpsReading reading)
    {
        // single reads are coming through here - this is a "bug" with Shiny.Locations
        if (this.gpsManager.CurrentListener == null)
            return;
        
        var within = reading.IsWithinPark(this.parkOptions);
        if (!within)
        {
            // shutter down
            this.Logger.LogInformation("Outside Wonderland, shutting down GPS");
            
            await this.gpsManager.StopListener();
            await this.notifications.Send(
                "Wonderland Goodbye",
                "GPS has been turned off to save battery - you will not receive real time ride updates"
            );
        }
    }
}