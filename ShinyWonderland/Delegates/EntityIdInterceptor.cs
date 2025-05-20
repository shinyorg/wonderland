using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Delegates;

public class EntityIdInterceptor(IConfiguration configuration) : IRequestMiddleware<GetEntityLiveDataHttpRequest, EntityLiveDataResponse>
{
    public Task<EntityLiveDataResponse> Process(IMediatorContext context, RequestHandlerDelegate<EntityLiveDataResponse> next, CancellationToken cancellationToken)
    {
        ((GetEntityLiveDataHttpRequest)context.Message).EntityID = configuration.GetValue<string>("Park:EntityId");
        return next();
    }
}