using ShinyWonderland.Contracts;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Features.Rides.Handlers;


[MediatorSingleton]
public class GetRideTimesRequestHandler(
    IOptions<ParkOptions> parkOptions
) : IRequestHandler<GetCurrentRideTimes, List<RideTime>>
{
    public async Task<List<RideTime>> Handle(GetCurrentRideTimes request, IMediatorContext context, CancellationToken cancellationToken)
    {
        // these calls are done sequentially as themepark api doesn't like multiple requests at the same time

        var liveData = await context.Request(
            new GetEntityLiveDataHttpRequest
            {
                EntityID = parkOptions.Value.EntityId
            },
            cancellationToken
        );

        var rides = await context.Request(new GetParkRidesRequest(), cancellationToken);

        var lastRides = await context.Request(new GetParkLastRiddenTimes(), cancellationToken);

        return MergeData(liveData, rides, lastRides);
    }
    

    static List<RideTime> MergeData(EntityLiveDataResponse liveData, IReadOnlyList<ParkRideInfo> rides, List<LastRideTime> lastRides)
    {
        var list = new List<RideTime>();
        foreach (var rideInfo in rides)
        {
            var live = liveData
                .LiveData?
                .FirstOrDefault(x => x.Id.Equals(rideInfo.Id, StringComparison.InvariantCultureIgnoreCase));

            var open = false;
            int? waitTime = null;
            int? paidWaitTime = null;
            Position? position = null;

            if (rideInfo is { Latitude: not null, Longitude: not null })
                position = new Position(rideInfo.Latitude!.Value, rideInfo.Longitude!.Value);

            if (live != null)
            {
                var wt = live.Queue?.Standby?.WaitTime;
                if (wt != null)
                    waitTime = Convert.ToInt32(wt);

                var pwt = live.Queue?.PaidStandby?.WaitTime;
                if (pwt != null)
                    paidWaitTime = Convert.ToInt32(pwt);

                open = live.Status == LiveStatusType.OPERATING;
            }

            var lastRide = lastRides?.FirstOrDefault(x => x.RideId == rideInfo.Id)?.Timestamp;
            
            list.Add(new RideTime(
                rideInfo.Id,
                rideInfo.Name,
                waitTime,
                paidWaitTime,
                position,
                open,
                lastRide
            ));
        }

        return list;
    }
}
