#if PLATFORM
using Microsoft.Extensions.Configuration;
using ShinyWonderland.Delegates;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace ShinyWonderland;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
#if DEBUG
        builder.Configuration.AddJsonPlatformBundle("debug");
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
        builder.AddMauiDevFlowAgent();
#else
        builder.Configuration.AddJsonPlatformBundle();
#endif
        
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseShiny()
            .UseShinyControls(x => x.AddDefaultMauiControlFeedback())
            .UseShinyShell(x => x
                .AddGeneratedMaps()
                .UseUxDiversDialogs()
            )
            .AddShinyMediator(
                x => x
                    .AddMediatorRegistry()
                    .AddGeneratedOpenApiClient()
                    .AddMauiPersistentCache()
                    .AddConnectivityBroadcaster()
                    .UseSentry(),
                false
            )
            .AddInfrastructureModules(
                new MealTimesModule(),
                new RideModule(),
                new ParkingModule()
            )
#if RELEASE
            // .UseSentry(x => x.Dsn = builder.Configuration["SentryDsn"]!)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services.Configure<ParkOptions>(builder.Configuration.GetSection("Park"));
        builder.Services.AddStronglyTypedLocalizations();
        builder.Services.AddGeneratedServices();
        
        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton(FileSystem.Current);
        builder.Services.AddSingleton(AppInfo.Current);
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddDatabase();
        builder.Services.AddNotifications();
        builder.Services.AddSingleton<MyGeofenceDelegate>();
        builder.Services.AddGps<MyGpsDelegate>();
        
        builder.Services.AddShinyStores();
        builder.Services.AddAppFunctions();
        var app = builder.Build();
        
        return app;
    }
}
#endif