using ShinyWonderland.Contracts;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Handlers;


[Service(ServiceLifetime.Singleton)]
public class ParkHourHandlers(
    IOptions<ParkOptions> parkOptions,
    TimeProvider timeProvider
) : IRequestHandler<GetCurrentParkHours, ParkHours>, IRequestHandler<GetUpcomingParkHours, ParkHours[]>
{
    // not cached
    public async Task<ParkHours> Handle(GetCurrentParkHours request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(timeProvider.GetLocalNow().LocalDateTime);
        var hours = await context
            .Request(
                new GetUpcomingParkHours(), 
                cancellationToken
            )
            .ConfigureAwait(false);

        var schedule = hours.FirstOrDefault(x => x.Date == date) ?? new ParkHours(date, null);
        return schedule;
    }
    
    // cached
    public async Task<ParkHours[]> Handle(GetUpcomingParkHours request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var upcoming = await context.Request(
            new GetEntityScheduleUpcomingHttpRequest
            {
                EntityID = parkOptions.Value.EntityId
            }, 
            cancellationToken
        );
        return upcoming
            .Schedule
            .Select(x =>
            {
                var date = DateOnly.Parse(x.Date);
                var isOpen = x.Type == ScheduleEntryType.OPERATING;
                TimeRange? timeRange = null;
                
                if (isOpen)
                {
                    var opening = TimeOnly.FromDateTime(x.OpeningTime.LocalDateTime);
                    var closing = TimeOnly.FromDateTime(x.ClosingTime.LocalDateTime);
                    timeRange = new(opening, closing);
                }
                return new ParkHours(date, timeRange);
            })
            .OrderBy(x => x.Date)
            .ToArray();
    }
}