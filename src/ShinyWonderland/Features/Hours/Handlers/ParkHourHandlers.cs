using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Features.Hours.Handlers;


[MediatorSingleton]
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
            new GetEntityScheduleHttpRequest
            {
                Id = parkOptions.Value.EntityId
            },
            cancellationToken
        );
        return upcoming
            .Schedule
            .Select(x =>
            {
                var date = DateOnly.Parse(x.Date);
                TimeRange? timeRange = null;

                if (x.Type == ScheduleEntryType.OPERATING && x.OpeningTime is { } openingTime && x.ClosingTime is { } closingTime)
                {
                    var opening = TimeOnly.FromDateTime(openingTime.LocalDateTime);
                    var closing = TimeOnly.FromDateTime(closingTime.LocalDateTime);
                    timeRange = new(opening, closing);
                }
                return new ParkHours(date, timeRange);
            })
            .OrderBy(x => x.Date)
            .ToArray();
    }
}
