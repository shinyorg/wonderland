namespace ShinyWonderland.Features.Parking.Tools;

[Description("Set the current location as the parking location")]
public record SetMyParking : ICommand;


[MediatorSingleton]
public partial class SetMyParkingHandler(
    AppSettings settings,
    IGpsManager gpsManager
) : ICommandHandler<SetMyParking>
{
    public async Task Handle(SetMyParking command, IMediatorContext context, CancellationToken cancellationToken)
    {
        if (settings.ParkingLocation == null)
        {
            var result = await gpsManager.GetCurrentPosition().ToTask(cancellationToken);
            if (result != null)
            {
                settings.ParkingLocation = result.Position;
            }
        }
    }
}