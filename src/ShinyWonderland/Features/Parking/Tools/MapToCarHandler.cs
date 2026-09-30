using Shiny.AppFunctions;

namespace ShinyWonderland.Features.Parking.Tools;

// OpensApp - launching the maps app needs the app in the foreground
[AppFunction("map_to_car", Title = "Find My Car", Description = "Opens a walking map to where you parked. Fails if you haven't set a parking location yet.", OpensApp = true)]
[AppShortcut("Where did I park at ${applicationName}", ShortTitle = "Find My Car", SystemImage = "car")]
public record MapToCar : IAppFunctionCommand;

[MediatorSingleton]
public partial class MapToCarHandler(AppSettings settings) : IAppFunctionCommandHandler<MapToCar>
{
    public async Task Handle(MapToCar command, IMediatorContext context, CancellationToken cancellationToken)
    {
        var parking = settings.ParkingLocation;
        if (parking == null)
            throw new AppFunctionException(AppFunctionErrorCode.NotFound, "You haven't set your parking location yet.");

        // app function handlers never run on the main thread
        var result = await MainThread.InvokeOnMainThreadAsync(() => Map.TryOpenAsync(
            parking.Latitude,
            parking.Longitude,
            new MapLaunchOptions
            {
                Name = "Where I Parked",
                NavigationMode = NavigationMode.Walking
            }
        ));

        if (!result)
            throw new AppFunctionException(AppFunctionErrorCode.AppError, "We were unable to open the map.");

        context.SayToAssistant("Here are walking directions to your car.");
    }
}
