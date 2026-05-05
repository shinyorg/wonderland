namespace ShinyWonderland.Features.AI.Handlers;

[Description("Opens a walking map with directions to a specific ride by its ID.")]
public record GetDirectionsToRide(
    [Description("The ID of the ride to get directions to")]
    string RideId
) : ICommand;

[MediatorSingleton]
public partial class DirectionsToRideHandler : ICommandHandler<GetDirectionsToRide>
{
    public async Task Handle(GetDirectionsToRide command, IMediatorContext context, CancellationToken cancellationToken)
    {
        var rides = await context.Request(new Features.Rides.Pages.GetCurrentRideTimes(), cancellationToken);
        var ride = rides.FirstOrDefault(r => r.Id.Equals(command.RideId, StringComparison.OrdinalIgnoreCase));

        if (ride == null)
            throw new InvalidOperationException($"No ride found with ID '{command.RideId}'.");

        if (ride.Position == null)
            throw new InvalidOperationException($"{ride.Name} does not have location data available.");

        var result = await Map.TryOpenAsync(
            ride.Position.Latitude,
            ride.Position.Longitude,
            new MapLaunchOptions
            {
                Name = ride.Name,
                NavigationMode = NavigationMode.Walking
            }
        );

        if (!result)
            throw new InvalidOperationException("Unable to open the map application.");
    }
}
