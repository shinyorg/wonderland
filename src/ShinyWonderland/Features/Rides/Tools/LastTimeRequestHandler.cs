using ShinyWonderland.Features.Rides.Handlers;

namespace ShinyWonderland.Features.Rides.Tools;

[Description("Checks when you last went on a specific ride. Tries an exact name match first, then a partial match.")]
public record GetLastRideTimeRequest(
    [Description("The name of the ride to look up (e.g. 'Leviathan', 'Behemoth')")]
    string RideName
) : IRequest<string>;

[MediatorSingleton]
public partial class LastTimeRequestHandler(TimeProvider timeProvider) : IRequestHandler<GetLastRideTimeRequest, string>
{
    public async Task<string> Handle(GetLastRideTimeRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var history = await context.Request(new GetRideHistory(null), cancellationToken);

        if (history.Count == 0)
            return "No ride history has been recorded yet.";

        // exact match first
        var match = history.FirstOrDefault(r =>
            r.RideName.Equals(request.RideName, StringComparison.OrdinalIgnoreCase));

        // partial match fallback
        match ??= history.FirstOrDefault(r =>
            r.RideName.Contains(request.RideName, StringComparison.OrdinalIgnoreCase));

        if (match == null)
            return $"No ride history found for '{request.RideName}'.";

        var ago = timeProvider.GetLocalNow() - match.Timestamp;
        var agoText = ago.TotalMinutes < 60
            ? $"{(int)ago.TotalMinutes} minutes ago"
            : $"{(int)ago.TotalHours} hours and {ago.Minutes} minutes ago";

        return $"You last rode {match.RideName} at {match.Timestamp:h:mm tt} ({agoText}).";
    }
}
