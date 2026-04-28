using Shiny.Notifications;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Delegates;


public class MyGpsDelegate(
    AppSettings appSettings,
    StringsLocalized strings,
    ILogger<MyGpsDelegate> logger,
    IOptions<ParkOptions> parkOptions,
    IGpsManager gpsManager,
    INotificationManager notificationManager,
    IMediator mediator
) : GpsDelegate(logger)
{
    protected override async Task OnGpsReading(GpsReading reading)
    {
        // single reads are coming through here - this is a "bug" with Shiny.Locations
        this.MinimumDistance = Distance.FromMeters(10);
        this.MinimumTime = TimeSpan.FromSeconds(10);

        try
        {
            var within = reading.IsWithinPark(parkOptions.Value);
            if (within)
            {
                await mediator.Publish(new GpsEvent(reading.Position));
            }
            else
            {
                // shutter down
                this.Logger.LogInformation("Outside Wonderland, shutting down GPS");

                appSettings.ParkingLocation = null;
                await gpsManager.StopListener();
                await notificationManager.Send(
                    parkOptions.Value.Name,
                    strings.LeaveParkNotificationMessage
                );
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error processing GPS reading");
        }
    }
}