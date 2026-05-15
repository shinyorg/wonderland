using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Features.Rides.Handlers;

public record GetParkRidesRequest : IRequest<IReadOnlyList<ParkRideInfo>>;

[SourceGenerateJsonConverter] // needed for cache
public partial record ParkRideInfo(string Id, string Name, double? Latitude, double? Longitude);

[MediatorSingleton]
public partial class GetParkRidesHandler(IOptions<ParkOptions> parkOptions)  : IRequestHandler<GetParkRidesRequest, IReadOnlyList<ParkRideInfo>>
{
    public async Task<IReadOnlyList<ParkRideInfo>> Handle(GetParkRidesRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var rides = await context.Request(
            new GetEntityChildrenHttpRequest
            {
                EntityID = parkOptions.Value.EntityId
            },
            cancellationToken
        );
        return rides.Children?
            .Where(x => x.EntityType == EntityType.ATTRACTION)
            .Select(x => new ParkRideInfo(
                x.Id,
                x.Name,
                x.Location?.Latitude,
                x.Location?.Longitude
            ))
            .ToList() ?? [];
    }
}