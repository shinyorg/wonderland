using ShinyWonderland.Features.Rides.Handlers;

namespace ShinyWonderland.Features.Rides.Tools;

[Description("Checks when you last went on a specific ride by its ID.")]
public record GetLastRideTimeRequest(
    [Description("The ID of the ride to look up")]
    string RideId
) : IRequest<string>;

[MediatorSingleton]
public partial class LastTimeRequestHandler(TimeProvider timeProvider) : IRequestHandler<GetLastRideTimeRequest, string>
{
    public async Task<string> Handle(GetLastRideTimeRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var history = await context.Request(new GetRideHistory(null), cancellationToken);

        if (history.Count == 0)
            return "No ride history has been recorded yet.";

        var match = history.FirstOrDefault(r =>
            r.RideId.Equals(request.RideId, StringComparison.OrdinalIgnoreCase));

        if (match == null)
            return $"No ride history found for ride ID '{request.RideId}'.";

        var ago = timeProvider.GetLocalNow() - match.Timestamp;
        var agoText = ago.TotalMinutes < 60
            ? $"{(int)ago.TotalMinutes} minutes ago"
            : $"{(int)ago.TotalHours} hours and {ago.Minutes} minutes ago";

        return $"You last rode {match.RideName} at {match.Timestamp:h:mm tt} ({agoText}).";
    }
}
