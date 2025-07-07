using ShinyWonderland.Contracts;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Handlers;


[Service(ServiceLifetime.Singleton)]
public class GetRideTimesRequestHandler(
    IOptions<ParkOptions> parkOptions
) : IRequestHandler<GetCurrentRideTimes, List<RideTime>>
{
    public async Task<List<RideTime>> Handle(GetCurrentRideTimes request, IMediatorContext context, CancellationToken cancellationToken)
    {
        // these calls are done sequentially as themepark api doesn't like multiple requests at the same time
        
        // these http calls are not cached - the outcome of this handler is which is why we don't do any filters or sorts here
        var liveDataTask = context.Request(
            new GetEntityLiveDataHttpRequest
            {
                EntityID = parkOptions.Value.EntityId
            },
            cancellationToken
        );

        var childDataTask = context.Request(
            new GetEntityChildrenHttpRequest
            {
                EntityID = parkOptions.Value.EntityId
            },
            cancellationToken
        );

        var lastRidesTask = context.Request(new GetParkLastRiddenTimes(), cancellationToken);
        
        await Task.WhenAll(liveDataTask, childDataTask, lastRidesTask).ConfigureAwait(false);

        return MergeData(liveDataTask.Result, childDataTask.Result, lastRidesTask.Result);
    }
    

    static List<RideTime> MergeData(EntityLiveDataResponse liveData, EntityChildrenResponse childData, List<LastRideTime> lastRides)
    {
        var list = new List<RideTime>();
        foreach (var rideInfo in childData.Children)
        {
            var live = liveData
                .LiveData
                .FirstOrDefault(x => x.Id.Equals(rideInfo.Id, StringComparison.InvariantCultureIgnoreCase));

            var open = false;
            int? waitTime = null;
            int? paidWaitTime = null;
            Position? position = null;

            if (rideInfo.Location != null)
                position = new Position(0, 0);

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

            var lastRide = lastRides.FirstOrDefault(x => x.RideId == rideInfo.Id)?.Timestamp;
            
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