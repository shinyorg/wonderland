using Microsoft.Extensions.DependencyInjection;

namespace ShinyWonderland;

public static class Registration
{
    public static IServiceCollection AddWonderlandLocalization(this IServiceCollection services)
    {
        services.AddStronglyTypedLocalizations();
        services.AddLocalization();
        return services;
    }
}