namespace ShinyWonderland;


public static class Extensions
{
    public static async Task<bool> IsWithinPark(
        this IGpsManager gpsManager, 
        ParkOptions parkOptions, 
        CancellationToken cancellationToken = default
    )
    {
#if DEBUG
        return true;
#else
        var reading = await gpsManager
            .GetCurrentPosition(cancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(15), cancellationToken)
            .ConfigureAwait(false);

        return reading?.IsWithinPark(parkOptions) ?? false;
#endif
    }


    public static bool IsWithinPark(this GpsReading reading, ParkOptions parkOptions)
    {
// #if DEBUG
//         return true;
// #else
        var distance = reading.Position.GetDistanceTo(parkOptions.CenterOfPark);
        var within = distance.TotalKilometers <= parkOptions.NotificationDistance.TotalKilometers;
        return within;
// #endif
    }
}