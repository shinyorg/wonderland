using Shiny.AppFunctions;
using ShinyWonderland.Features.Rides.Handlers;

namespace ShinyWonderland.Features.Rides.Tools;

[AppFunction("get_last_ride_time", Title = "Last Time On Ride", Description = "Checks when you last went on a specific ride.")]
[AppShortcut("When did I last ride at ${applicationName}", ShortTitle = "Last Ridden", SystemImage = "clock.arrow.circlepath")]
public record GetLastRideTimeRequest(
    [property: AppParameter(Title = "Ride", Description = "The ride to look up")]
    ParkRide Ride
) : IAppFunctionRequest<string>;

[MediatorSingleton]
public partial class LastTimeRequestHandler(TimeProvider timeProvider) : IAppFunctionRequestHandler<GetLastRideTimeRequest, string>
{
    public async Task<string> Handle(GetLastRideTimeRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var result = await this.GetLastRideTime(request, context, cancellationToken);
        context.SayToAssistant(result);
        return result;
    }


    async Task<string> GetLastRideTime(GetLastRideTimeRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var history = await context.Request(new GetRideHistory(null), cancellationToken);

        if (history.Count == 0)
            return "No ride history has been recorded yet.";

        var match = history.FirstOrDefault(r =>
            r.RideId.Equals(request.Ride.Id, StringComparison.OrdinalIgnoreCase));

        if (match == null)
            return $"You haven't been on {request.Ride.Name} yet.";

        var ago = timeProvider.GetLocalNow() - match.Timestamp;
        var agoText = ago.TotalMinutes < 60
            ? $"{(int)ago.TotalMinutes} minutes ago"
            : $"{(int)ago.TotalHours} hours and {ago.Minutes} minutes ago";

        return $"You last rode {match.RideName} at {match.Timestamp:h:mm tt} ({agoText}).";
    }
}
