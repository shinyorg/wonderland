using Shiny.AppFunctions;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Features.Parking.Tools;

[AppFunction("set_my_parking", Title = "Save Parking Spot", Description = "Saves your current location as your parking location. Fails if you aren't at the park.")]
[AppShortcut("Remember where I parked at ${applicationName}", ShortTitle = "Save Parking", SystemImage = "parkingsign")]
public record SetMyParking : IAppFunctionCommand;


[MediatorSingleton]
public partial class SetMyParkingHandler(
    AppSettings settings,
    IOptions<ParkOptions> parkOptions,
    IGpsManager gpsManager
) : IAppFunctionCommandHandler<SetMyParking>
{
    public async Task Handle(SetMyParking command, IMediatorContext context, CancellationToken cancellationToken)
    {
        if (settings.ParkingLocation != null)
        {
            context.SayToAssistant("Your parking location is already saved.");
            return;
        }

        var result = await gpsManager.GetCurrentPosition(cancellationToken);
        if (result == null)
            throw new AppFunctionException(AppFunctionErrorCode.AppError, "I couldn't get your current location.");

        // same rule as the parking page
        if (!result.IsWithinPark(parkOptions.Value))
            throw new AppFunctionException(AppFunctionErrorCode.Denied, $"You aren't close enough to {parkOptions.Value.Name} to save a parking spot.");

        settings.ParkingLocation = result.Position;

        // the user may have asked from inside the app - refresh the parking page if it is showing
        await context.Publish(new ParkingLocationChangedEvent(result.Position), cancellationToken: cancellationToken);
        context.SayToAssistant("I saved your parking location.");
    }
}
