using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public static class Extensions
{
    public static Task<(IMediatorContext Context, EntityLiveDataResponse Result)> GetWonderlandData(
        this IMediator mediator,
        bool forceRefresh,
        CancellationToken cancellationToken
    ) => mediator.Request(
        new GetEntityLiveDataHttpRequest(), // entity ID is set by middleware
        cancellationToken,
        ctx =>
        {
            if (forceRefresh)
                ctx.ForceCacheRefresh();
        }
    );


    public static async Task<bool> IsWithinPark(this IGpsManager gpsManager, ParkOptions parkOptions)
    {
        var reading = await gpsManager
            .GetCurrentPosition()
            .Timeout(TimeSpan.FromSeconds(15))
            .ToTask();

        return reading.IsWithinPark(parkOptions);
    }


    public static bool IsWithinPark(this GpsReading reading, ParkOptions parkOptions)
    {
        var distance = reading.Position.GetDistanceTo(parkOptions.CenterOfPark);
        var within = distance.TotalKilometers <= parkOptions.NotificationDistance.TotalKilometers;
        return within;
    }
}