using Shiny.Mediator.Caching;
using ShinyWonderland.ThemeParksApi;


namespace ShinyWonderland
{
    public static class Extensions
    {
        // https://api.themeparks.wiki/docs/v1.yaml
        // https://api.themeparks.wiki/v1/entity/66f5d97a-a530-40bf-a712-a6317c96b06d/live
        public static Task<(IMediatorContext Context, EntityLiveDataResponse Result)> GetWonderlandData(
            this IMediator mediator, 
            bool forceRefresh,
            CancellationToken cancellationToken
        ) => mediator.Request(
            new GetEntityLiveDataHttpRequest
            {
                EntityID = Constants.ParkId,
            },
            cancellationToken,
            ctx =>
            {
                if (forceRefresh)
                    ctx.ForceCacheRefresh();
            }
        );
    }
}