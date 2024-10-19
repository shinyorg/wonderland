using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;

public static class Extensions
{
    // https://api.themeparks.wiki/docs/v1.yaml
    // https://api.themeparks.wiki/v1/entity/66f5d97a-a530-40bf-a712-a6317c96b06d/live
    public static Task<EntityLiveDataResponse> GetWonderlandData(this IMediator mediator, CancellationToken cancellationToken) => mediator.Request(
        new GetEntityLiveDataHttpRequest { EntityID = "66f5d97a-a530-40bf-a712-a6317c96b06d" },
        cancellationToken
    );
}