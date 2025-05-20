using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public static class Extensions
{
    public static Task<(IMediatorContext Context, EntityLiveDataResponse Result)> GetWonderlandData(
        this IMediator mediator,
        bool forceRefresh,
        CancellationToken cancellationToken
    ) => mediator.Request(
        new GetEntityLiveDataHttpRequest(), // entity ID is set by middleware
        cancellationToken,
        ctx =>
        {
            if (forceRefresh)
                ctx.ForceCacheRefresh();
        }
    );
}