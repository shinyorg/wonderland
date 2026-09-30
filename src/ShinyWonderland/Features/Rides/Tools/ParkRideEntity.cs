using Shiny.AppFunctions;
using ShinyWonderland.Features.Rides.Handlers;

namespace ShinyWonderland.Features.Rides.Tools;

// Lets Siri/Shortcuts show a ride picker and Android agents find ride ids (via the generated search_ride function)
// instead of the assistant having to know the park's ride ids
[AppEntity("ride", Title = "Ride")]
public record ParkRide(string Id, string Name);


public class ParkRideQuery(IMediator mediator) : IAppEntityQuery<ParkRide>
{
    public async Task<IReadOnlyList<ParkRide>> GetByIds(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var rides = await this.GetRides(cancellationToken);
        return rides
            .Where(x => ids.Contains(x.Id, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }


    public async Task<IReadOnlyList<ParkRide>> Search(string text, CancellationToken cancellationToken)
    {
        var rides = await this.GetRides(cancellationToken);
        return rides
            .Where(x => x.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }


    public Task<IReadOnlyList<ParkRide>> Suggested(CancellationToken cancellationToken)
        => this.GetRides(cancellationToken);


    async Task<IReadOnlyList<ParkRide>> GetRides(CancellationToken cancellationToken)
    {
        var rides = await mediator.Request(new GetParkRidesRequest(), cancellationToken);
        return rides
            .Result
            .OrderBy(x => x.Name)
            .Select(x => new ParkRide(x.Id, x.Name))
            .ToList();
    }
}
