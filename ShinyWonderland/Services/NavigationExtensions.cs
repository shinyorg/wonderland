using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShinyWonderland.Services.Impl;

namespace ShinyWonderland.Services;


//https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0
public static class NavigationExtensions
{
    public static MauiAppBuilder UseShinyNavigation(this MauiAppBuilder builder, Action<ShinyNavigationBuilder> navBuilderAction)
    {
        var navBuilder = new ShinyNavigationBuilder();
        navBuilderAction.Invoke(navBuilder);
        navBuilder.RegisterDependencies(builder.Services);
        
        builder.Services.AddSingletonAsImplementedInterfaces<ShinyShellNavigator>();
        builder.Services.TryAddSingleton(navBuilder);
        return builder;
    }
}

public sealed class ShinyNavigationBuilder
{
    readonly Dictionary<string, (bool RegisterRoute, Type PageType, Type ViewModelType)> typeMap = new();
    
    public ShinyNavigationBuilder Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPage, 
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel
    >(string? route = null, bool registerRoute = true)
        where TPage : Page
        where TViewModel : class, INotifyPropertyChanged
    {
        route ??= typeof(TPage).Name;
        this.typeMap[route] = (registerRoute, typeof(TPage), typeof(TViewModel));
        return this;
    }
    

    public Type? GetViewModelTypeForPage(Type pageType)
    {
        foreach (var pair in this.typeMap)
        {
            if (pair.Value.PageType == pageType) 
                return pair.Value.ViewModelType;
        }
        return null;
    }

    
    internal void RegisterDependencies(IServiceCollection services)
    {
        foreach (var pair in this.typeMap)
        {
            services.AddTransient(pair.Value.PageType);
            services.AddTransient(pair.Value.ViewModelType);
            if (pair.Value.RegisterRoute)
            {
                Routing.RegisterRoute(
                    pair.Key,
                    new ShinyRouteFactory(
                        pair.Value.PageType,
                        pair.Value.ViewModelType
                    )
                );
            }
        }
    }
}

public class ShinyRouteFactory(Type pageType, Type viewModelType) : RouteFactory
{
    public override Element GetOrCreate() => throw new NotImplementedException();
    public override Element GetOrCreate(IServiceProvider services)
    {
        var page = (Page)services.GetRequiredService(pageType);
        page.BindingContext = services.GetRequiredService(viewModelType);
        return page;
    }
}