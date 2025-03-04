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
                ForceRefresh = forceRefresh
            },
            cancellationToken
        );
    }
}

namespace ShinyWonderland.ThemeParksApi
{
    public partial class GetEntityLiveDataHttpRequest : ICacheControl
    {
        public bool ForceRefresh { get; set; }
        public TimeSpan? AbsoluteExpiration { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
    }
}