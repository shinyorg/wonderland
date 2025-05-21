using CommunityToolkit.Maui;
using Shiny.Jobs;
using ShinyWonderland.Delegates;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseShiny()
            .UsePrism(
                new DryIocContainerExtension(),
                prism => prism.CreateWindow("NavigationPage/MainPage")
            )
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Configuration.AddJsonPlatformBundle();
        
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#endif

        builder.Services.AddShinyMediator(x => x
            .AddRequestMiddleware<GetEntityLiveDataHttpRequest, EntityLiveDataResponse, EntityIdInterceptor>()
            .AddMauiPersistentCache()
            .AddConnectivityBroadcaster()
            .UseMaui()
        );
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddGps();
        builder.Services.AddJob(
            typeof(MyJob),
            requiredNetwork: InternetAccess.Any,
            runInForeground: true
        );
        builder.Services.AddNotifications();

        builder.Services.AddShinyService<AppSettings>();
        builder.Services.RegisterForNavigation<MainPage, MainViewModel>();
        builder.Services.RegisterForNavigation<SettingsPage, SettingsViewModel>();
        var app = builder.Build();

        return app;
    }
}