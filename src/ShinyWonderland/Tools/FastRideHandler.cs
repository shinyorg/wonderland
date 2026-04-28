using ShinyWonderland.Contracts;

namespace ShinyWonderland.Tools;

[Description("Gets current ride wait times. Can find the shortest queue or the wait time for a specific ride by name.")]
public record GetRideWaitTimes(
    [Description("Optional ride name to look up. Leave null to return all open rides sorted by shortest wait time.")]
    string? RideName = null
) : IRequest<string>;

[MediatorSingleton]
public partial class FastRideHandler : IRequestHandler<GetRideWaitTimes, string>
{
    [Cache(AbsoluteExpirationSeconds = 120)]
    public async Task<string> Handle(GetRideWaitTimes request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var rides = await context.Request(new GetCurrentRideTimes(), cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.RideName))
        {
            var match = rides.FirstOrDefault(r =>
                r.Name.Contains(request.RideName, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                return $"No ride found matching '{request.RideName}'.";

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