#if PLATFORM
#if DEBUG
using MauiDevFlow.Agent;
#endif
using Microsoft.Extensions.Configuration;
using Shiny.Jobs;
using ShinyWonderland.Delegates;

namespace ShinyWonderland;


public static class MauiProgram
{
    
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder.Configuration.AddJsonPlatformBundle();
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
        builder.AddMauiDevFlowAgent();
#endif
        
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseShiny()
            .UseShinyControls()
            .UseShinyShell(x => x
                .AddGeneratedMaps()
                .UseUxDiversDialogs()
            )
            .AddShinyMediator(
                x => x
                    .AddMediatorRegistry()
                    .AddGeneratedOpenApiClient()
                    .AddThrottleEventMiddleware()
                    .AddMauiPersistentCache()
                    .AddConnectivityBroadcaster()
                    .UseSentry()
                    .UseMaui(false),
                false
            )
#if RELEASE
            .UseSentry(opts =>
            {
                opts.Dsn = builder.Configuration["SentryDsn"]!;
                // DiagnosticListener.AllListeners.Subscribe(
                //     x => x.Subscribe()
                // );
            })
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.Configure<ParkOptions>(builder.Configuration.GetSection("Park"));
        builder.Services.Configure<MealTimeOptions>(builder.Configuration.GetSection("MealPass"));
        builder.Services.AddWonderlandLocalization();
        
        builder.Services.AddGeneratedServices();
        builder.Services.AddShinyService<AppSettings>();

        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddDatabase();
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
}
#endif