namespace ShinyWonderland.Services;


public class MyGpsDelegate : GpsDelegate
{
    readonly IGpsManager gpsManager;
    readonly ParkOptions parkOptions;
    
    public MyGpsDelegate(
        ILogger<MyGpsDelegate> logger,
        IConfiguration config,
        IOptions<ParkOptions> parkOptions,
        IGpsManager gpsManager
    ) : base(logger)
    {
        this.MinimumDistance = Distance.FromMeters(10);
        this.MinimumTime = TimeSpan.FromSeconds(10);

        var lat = config.GetValue<double>("Park:Latitude");
        var lon = config.GetValue<double>("Park:Longitude");
        this.gpsManager = gpsManager;
        this.parkOptions = parkOptions.Value;
    }

    
    protected override async Task OnGpsReading(GpsReading reading)
    {
        var dist = reading.Position.GetDistanceTo(this.parkOptions.CenterOfPark);
        if (dist.TotalKilometers >= 1)
        {
            // shutter down
            this.Logger.LogInformation("Outside Wonderland, shutting down GPS");
            await this.gpsManager.StopListener();
        }
    }
}