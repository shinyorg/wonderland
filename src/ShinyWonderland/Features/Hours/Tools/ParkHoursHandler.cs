using Shiny.AppFunctions;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Features.Hours.Tools;

[AppFunction(
    "get_park_hours",
    Title = "Park Hours",
    Description = "Gets park hours for today or a specific date. If the park is closed on that date, returns the next open date and its hours."
)]
[AppShortcut("When is ${applicationName} open", ShortTitle = "Park Hours", SystemImage = "calendar")]
public record GetParkHoursRequest(
    [property: AppParameter(Title = "Date", Description = "Optional date to check park hours for. If not provided, returns today's hours.")]
    DateTimeOffset? Date = null
) : IAppFunctionRequest<string>;

[MediatorSingleton]
public partial class ParkHoursHandler(TimeProvider timeProvider) : IAppFunctionRequestHandler<GetParkHoursRequest, string>
{
    public async Task<string> Handle(GetParkHoursRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var result = await this.GetParkHours(request, context, cancellationToken);
        context.SayToAssistant(result);
        return result;
    }


    async Task<string> GetParkHours(GetParkHoursRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var upcoming = await context.Request(new GetUpcomingParkHours(), cancellationToken);

        var targetDate = DateOnly.FromDateTime((request.Date ?? timeProvider.GetLocalNow()).Date);

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
