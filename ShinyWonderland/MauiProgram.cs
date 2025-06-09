using Shiny.Jobs;
using ShinyWonderland.Delegates;
using ShinyWonderland.ThemeParksApi;
using SQLite;

namespace ShinyWonderland;


public static class MauiProgram
{
    #if PLATFORM
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseShiny()
            .UseShinyShell(x => x.AddGeneratedMaps())
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

        builder.Services.Configure<ParkOptions>(builder.Configuration.GetSection("Park"));
        builder.Services.Configure<MealTimeOptions>(builder.Configuration.GetSection("MealTime"));
        
        builder.Configuration.AddJsonPlatformBundle();
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#endif
        builder.Services.AddDiscoveredMediatorHandlersFromShinyWonderland();
        builder.Services.AddSingleton<CoreServices>();
        
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<SQLiteAsyncConnection>(_ =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return new SQLiteAsyncConnection(Path.Combine(appData, "ShinyWonderland.db"));
        });
        builder.Services.AddShinyService<AppSettings>();
        builder.Services.AddNotifications();
        builder.Services.AddGeofencing<MyGeofenceDelegate>();
        builder.Services.AddGps<MyGpsDelegate>();
        builder.Services.AddJob(
            typeof(RideTimeJob),
            requiredNetwork: InternetAccess.Any,
            runInForeground: true
        );
        builder.Services.AddJob(
            typeof(MealTimeJob),
            runInForeground: true
        );
        var app = builder.Build();

        return app;
    }
    #endif
}