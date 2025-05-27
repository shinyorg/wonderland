using CommunityToolkit.Maui;
using Shiny.Jobs;
using ShinyWonderland.Delegates;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;


public static class MauiProgram
{
    #if PLATFORM
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseShiny()
            .UseMauiMaps()
            .UseShinyShell(x => x
                .Add<MainPage, MainViewModel>(registerRoute: false) // registered in Shell XAML
                .Add<SettingsPage, SettingsViewModel>()
                .Add<ParkingPage, ParkingViewModel>()
                .Add<HoursPage, HoursViewModel>()
            )
            .AddShinyMediator(x => x
                .AddMauiPersistentCache()
                .AddConnectivityBroadcaster()
                .AddHttpClient()
                .UseMaui(false),
                false
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
        builder.Services.AddDiscoveredMediatorHandlersFromShinyWonderland();
        builder.Services.AddSingleton<CoreServices>();
        builder.Services.Configure<ParkOptions>(builder.Configuration.GetSection("Park"));
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddShinyService<AppSettings>();
        builder.Services.AddNotifications();
        builder.Services.AddGeofencing<MyGeofenceDelegate>();
        builder.Services.AddGps<MyGpsDelegate>();
        builder.Services.AddJob(
            typeof(MyJob),
            requiredNetwork: InternetAccess.Any,
            runInForeground: true
        );
        var app = builder.Build();

        return app;
    }
    #endif
}