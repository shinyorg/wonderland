using ShinyWonderland.Contracts;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Handlers;


[SingletonHandler]
public class GetRideTimesRequestHandler(
    IOptions<ParkOptions> parkOptions
) : IRequestHandler<GetCurrentRideTimes, List<RideTime>>
{
    public async Task<List<RideTime>> Handle(GetCurrentRideTimes request, IMediatorContext context, CancellationToken cancellationToken)
    {
        // these http calls are not cached - the outcome of this handler is which is why we don't do any filters or sorts here
        var liveData = context.Request(
            new GetEntityLiveDataHttpRequest
            {
                EntityID = parkOptions.Value.EntityId
            },
            cancellationToken
        );
        
        var childData = context.Request(
            new GetEntityChildrenHttpRequest
            {
                EntityID = parkOptions.Value.EntityId
            },
            cancellationToken
        );
        
        await Task
            .WhenAll(liveData, childData)
            .ConfigureAwait(false);

        var list = new List<RideTime>();
        foreach (var rideInfo in childData.Result.Children)
        {
            var live = liveData
                .Result
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
                var wt = live.Queue?.PaidStandby?.WaitTime;
                if (wt != null)
                    waitTime = Convert.ToInt32(wt);
                
                var pwt = live.Queue?.PaidStandby?.WaitTime;
                if (pwt != null)
                    paidWaitTime = Convert.ToInt32(pwt);
                
                open = live.Status == LiveStatusType.OPERATING;
            }

            list.Add(new RideTime(
                rideInfo.Id,
                rideInfo.Name,
                waitTime,
                paidWaitTime,
                position,
                open
            ));
        }

        return list;
    }
}