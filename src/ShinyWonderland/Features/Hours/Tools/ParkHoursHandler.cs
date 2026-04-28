using ShinyWonderland.Contracts;

namespace ShinyWonderland.Features.Hours.Tools;

[Description("Gets park hours for today or a specific date. If the park is closed on that date, returns the next open date and its hours.")]
public record GetParkHoursRequest(
    [Description("Optional date to check park hours for (e.g. '2026-05-01'). If not provided, returns today's hours.")]
    string? Date = null
) : IRequest<string>;

[MediatorSingleton]
public partial class ParkHoursHandler(TimeProvider timeProvider) : IRequestHandler<GetParkHoursRequest, string>
{
    public async Task<string> Handle(GetParkHoursRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var upcoming = await context.Request(new GetUpcomingParkHours(), cancellationToken);

        DateOnly targetDate;
        if (!string.IsNullOrWhiteSpace(request.Date) && DateOnly.TryParse(request.Date, out var parsed))
            targetDate = parsed;
        else
            targetDate = DateOnly.FromDateTime(timeProvider.GetLocalNow().Date);

        var hours = upcoming.FirstOrDefault(h => h.Date == targetDate);

        if (hours is { IsOpen: true })
            return $"The park is open on {hours.Date:ddd MMM d} from {hours.Hours!.Open:h:mm tt} to {hours.Hours.Closed:h:mm tt}.";

        // Park is closed on that date — find next open date
        var nextOpen = upcoming
            .Where(h => h.Date >= targetDate && h.IsOpen)
            .OrderBy(h => h.Date)
            .FirstOrDefault();

        var closedMsg = $"The park is closed on {targetDate:ddd MMM d}.";
        if (nextOpen != null)
            closedMsg += $" The next open date is {nextOpen.Date:ddd MMM d} from {nextOpen.Hours!.Open:h:mm tt} to {nextOpen.Hours.Closed:h:mm tt}.";

        return closedMsg;
    }
}
