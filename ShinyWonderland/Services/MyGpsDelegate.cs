using Shiny.Locations;

namespace ShinyWonderland.Services;


public class MyGpsDelegate : GpsDelegate
{
    readonly Position parkCenter;
    
    public MyGpsDelegate(
        ILogger<MyGpsDelegate> logger,
        IConfiguration config
    ) : base(logger)
    {
        this.MinimumDistance = Distance.FromMeters(10);
        this.MinimumTime = TimeSpan.FromSeconds(10);

        var lat = config.GetValue<double>("Park:Latitude");
        var lon = config.GetValue<double>("Park:Longitude");
        this.parkCenter = new(lat, lon);
    }

    
    protected override async Task OnGpsReading(GpsReading reading)
    {
        var dist = reading.Position.GetDistanceTo(this.parkCenter);
        if (dist.TotalKilometers > 1)
        {
            // shutter down
        }
    }
}