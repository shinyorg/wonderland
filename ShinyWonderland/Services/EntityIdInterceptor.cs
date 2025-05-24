using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Services;


// used to set the entity ID on live data requests so that we don't need to inject configuration on the extension
public class EntityIdInterceptor(IOptions<ParkOptions> parkOptions) : IRequestMiddleware<GetEntityLiveDataHttpRequest, EntityLiveDataResponse>
{
    public Task<EntityLiveDataResponse> Process(IMediatorContext context, RequestHandlerDelegate<EntityLiveDataResponse> next, CancellationToken cancellationToken)
    {
        ((GetEntityLiveDataHttpRequest)context.Message).EntityID = parkOptions.Value.EntityId;
        return next();
    }
}