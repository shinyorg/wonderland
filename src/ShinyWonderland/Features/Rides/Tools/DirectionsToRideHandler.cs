using Shiny.AppFunctions;

namespace ShinyWonderland.Features.Rides.Tools;

// OpensApp - launching the maps app needs the app in the foreground
[AppFunction("directions_to_ride", Title = "Directions To Ride", Description = "Opens a walking map with directions to a specific ride.", OpensApp = true)]
[AppShortcut("Take me to a ride in ${applicationName}", ShortTitle = "Directions", SystemImage = "figure.walk")]
public record GetDirectionsToRide(
    [property: AppParameter(Title = "Ride", Description = "The ride to get directions to")]
    ParkRide Ride
) : IAppFunctionCommand;

[MediatorSingleton]
public partial class DirectionsToRideHandler : IAppFunctionCommandHandler<GetDirectionsToRide>
{
    public async Task Handle(GetDirectionsToRide command, IMediatorContext context, CancellationToken cancellationToken)
    {
        var rides = await context.Request(new GetCurrentRideTimes(), cancellationToken);
        var ride = rides.FirstOrDefault(r => r.Id.Equals(command.Ride.Id, StringComparison.OrdinalIgnoreCase));

        if (ride == null)
            throw new AppFunctionException(AppFunctionErrorCode.NotFound, $"I couldn't find {command.Ride.Name}.");

        if (ride.Position == null)
            throw new AppFunctionException(AppFunctionErrorCode.NotFound, $"{ride.Name} does not have location data available.");

        // app function handlers never run on the main thread
        var result = await MainThread.InvokeOnMainThreadAsync(() => Map.TryOpenAsync(
            ride.Position.Latitude,
            ride.Position.Longitude,
            new MapLaunchOptions
            {
                Name = ride.Name,
                NavigationMode = NavigationMode.Walking
            }
        ));

        if (!result)
            throw new AppFunctionException(AppFunctionErrorCode.AppError, "Unable to open the map application.");

        context.SayToAssistant($"Here are walking directions to {ride.Name}.");
    }
}
