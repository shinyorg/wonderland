using Shiny.Speech;

namespace ShinyWonderland.Features.Parking.Tools;

[Description("Opens a map to where you are parked - does nothing if you the user hasn't parked yet")]
public record MapToCar : ICommand;

[MediatorSingleton]
public partial class MapToCarHandler(
    ITextToSpeechService textToSpeech,
    AppSettings settings
) : ICommandHandler<MapToCar>
{
    public async Task Handle(MapToCar command, IMediatorContext context, CancellationToken cancellationToken)
    {
        if (settings.ParkingLocation == null)
        {
            await textToSpeech.SpeakAsync("You haven't parked yet", cancellationToken: cancellationToken);
            return;
        }
        var result = await Map.TryOpenAsync(
            settings.ParkingLocation.Latitude, 
            settings.ParkingLocation.Longitude, 
            new MapLaunchOptions
            {
                Name = "Where I Parked",
                NavigationMode = NavigationMode.Walking
            }
        );
        if (!result)
            await textToSpeech.SpeakAsync("We were unable to open the map", cancellationToken: cancellationToken);
    }
}