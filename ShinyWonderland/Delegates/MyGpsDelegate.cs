using Shiny.Notifications;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Delegates;


public class MyGpsDelegate : GpsDelegate
{
    readonly CoreServices services;
    
    
    public MyGpsDelegate(
        ILogger<MyGpsDelegate> logger,
        CoreServices services
    ) : base(logger)
    {
        this.MinimumDistance = Distance.FromMeters(10);
        this.MinimumTime = TimeSpan.FromSeconds(10);
        this.services = services;
    }

    
    protected override async Task OnGpsReading(GpsReading reading)
    {
        // single reads are coming through here - this is a "bug" with Shiny.Locations
        if (this.services.Gps.CurrentListener == null)
            return;
        
        var within = reading.IsWithinPark(this.services.ParkOptions.Value);
        if (within)
        {
            await this.services.Mediator.Publish(new GpsEvent(reading.Position));
        }
        else
        {
            // shutter down
            this.Logger.LogInformation("Outside Wonderland, shutting down GPS");

            this.services.AppSettings.ParkingLocation = null;
            await this.services.Gps.StopListener();
            await this.services.Notifications.Send(
                "Wonderland",
                "Hope you had fun.  GPS has been turned off to save battery, you will not receive real time ride updates, and parking has been cleared"
            );
        }
    }
}