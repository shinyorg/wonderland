using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShinyWonderland.Services.Impl;

namespace ShinyWonderland.Services;


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
    readonly Dictionary<string, (Type PageType, Type ViewModelType)> typeMap = new();
    
    public ShinyNavigationBuilder Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPage, 
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel
    >(string? route = null)
        where TPage : Page
        where TViewModel : class, INotifyPropertyChanged
    {
        route ??= typeof(TPage).Name;
        this.typeMap[route] = (typeof(TPage), typeof(TViewModel));
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

    
    public Type? GetViewModelTypeForRoute(string route)
    {
        foreach (var pair in this.typeMap)
        {
            if (pair.Key.Equals(route, StringComparison.InvariantCultureIgnoreCase)) 
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
            Routing.RegisterRoute(pair.Key, pair.Value.PageType);
        }
    }
}