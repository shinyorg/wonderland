using Shiny.Notifications;

namespace ShinyWonderland.Services;

public record CoreServices(
    IMediator Mediator,
    IOptions<ParkOptions> ParkOptions,
    AppSettings AppSettings,
    INavigator Navigator,
    TimeProvider TimeProvider,
    IGpsManager Gps,
    INotificationManager Notifications
)
{
    public async Task<(bool IsWithinPark, Position? Position)> TrySetParking(CancellationToken cancellationToken)
    {
        var reading = await this.Gps.GetCurrentPosition().ToTask(cancellationToken);
        
        if (reading.IsWithinPark(this.ParkOptions.Value))
        {
            this.AppSettings.ParkingLocation = reading.Position;
            return (true, reading.Position);
        }
        return (false, null);
    }
    
    
    public Task<bool> IsUserWithinPark(CancellationToken cancellationToken = default) 
        => this.Gps.IsWithinPark(this.ParkOptions.Value, cancellationToken);
}