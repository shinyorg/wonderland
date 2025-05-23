using CommunityToolkit.Maui;
using Shiny.Jobs;
using ShinyWonderland.Delegates;
using ShinyWonderland.Services;
using ShinyWonderland.Services.Impl;
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
            .UseShinyNavigation(x => x
                .Add<MainPage, MainViewModel>()
                .Add<SettingsPage, SettingsViewModel>()
            )
            .AddShinyMediator(x => x
                .AddRequestMiddleware<GetEntityLiveDataHttpRequest, EntityLiveDataResponse, EntityIdInterceptor>()
                .AddMauiPersistentCache()
                .AddConnectivityBroadcaster()
                .UseMaui()
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

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddShinyService<AppSettings>();
        builder.Services.AddNotifications();
        builder.Services.AddGps<MyGpsDelegate>();
        builder.Services.AddJob(
            typeof(MyJob),
            requiredNetwork: InternetAccess.Any,
            runInForeground: true
        );
        var app = builder.Build();

        return app;
    }
}