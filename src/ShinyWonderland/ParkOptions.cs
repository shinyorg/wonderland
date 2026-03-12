namespace ShinyWonderland;


public class ParkOptions
{
    public string Name { get; set; }
    public string EntityId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int NotificationDistanceMeters { get; set; }
    public int MapStartZoomDistanceMeters { get; set; }

    public Position CenterOfPark => new(this.Latitude, this.Longitude);
    public Distance NotificationDistance => Distance.FromMeters(this.NotificationDistanceMeters);
}