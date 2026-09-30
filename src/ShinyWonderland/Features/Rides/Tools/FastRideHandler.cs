using Shiny.AppFunctions;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Features.Rides.Tools;

[AppFunction(
    "get_ride_wait_times",
    Title = "Ride Wait Times",
    Description = "Gets current ride wait times. Can find the shortest queue or the wait time for a specific ride."
)]
[AppShortcut("What are the wait times at ${applicationName}", ShortTitle = "Wait Times", SystemImage = "clock")]
[AppShortcut("What has the shortest line at ${applicationName}")]
public record GetRideWaitTimes(
    [property: AppParameter(Title = "Ride", Description = "Optional ride to look up. Leave empty to return all open rides sorted by shortest wait time.")]
    ParkRide? Ride = null
) : IAppFunctionRequest<string>;

// no [Cache] here - a cache hit skips the handler and the assistant would get no dialog. GetCurrentRideTimes is cached.
[MediatorSingleton]
public partial class FastRideHandler : IAppFunctionRequestHandler<GetRideWaitTimes, string>
{
    public async Task<string> Handle(GetRideWaitTimes request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var result = await GetWaitTimes(request, context, cancellationToken);
        context.SayToAssistant(result);
        return result;
    }


    static async Task<string> GetWaitTimes(GetRideWaitTimes request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var rides = await context.Request(new GetCurrentRideTimes(), cancellationToken);

        if (request.Ride != null)
        {
            var match = rides.FirstOrDefault(r =>
                r.Id.Equals(request.Ride.Id, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                throw new AppFunctionException(AppFunctionErrorCode.NotFound, $"There are no wait times for {request.Ride.Name}.");

            if (!match.IsOpen)
                return $"{match.Name} is currently closed.";

            return match.WaitTimeMinutes.HasValue
                ? $"{match.Name} has a {match.WaitTimeMinutes} minute wait."
                : $"{match.Name} is open but has no posted wait time.";
        }

        var openRides = rides
            .Where(r => r.IsOpen && r.WaitTimeMinutes.HasValue)
            .OrderBy(r => r.WaitTimeMinutes)
            .ToList();

        if (openRides.Count == 0)
            return "No rides are currently reporting wait times.";

        var lines = openRides.Select(r => $"- {r.Name}: {r.WaitTimeMinutes} min");
        return $"Open rides sorted by shortest wait:\n{string.Join('\n', lines)}";
    }
}
