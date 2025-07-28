using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Shiny.Extensions.Stores;
using Shiny.Jobs;
using ShinyWonderland.Delegates;
using SQLite;

namespace ShinyWonderland;


public static class MauiProgram
{
    #if PLATFORM
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
#if DEBUG
        builder.Configuration.AddJsonPlatformBundle();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#else
        builder.Configuration.AddJsonPlatformBundle("release");
#endif
        
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseShiny()
            .UseShinyShell(x => x.AddGeneratedMaps())
            .UseSentry(opts =>
            {
                opts.Dsn = builder.Configuration["Sentry:Dsn"]!;
                // DiagnosticListener.AllListeners.Subscribe(
                //     x => x.Subscribe()
                // );
#if DEBUG
                opts.Debug = true;
#endif
            })
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
       

        builder.Services.AddGeneratedServices();
        builder.Services.AddStronglyTypedLocalizations();
        builder.Services.AddPersistentService<AppSettings>();

        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<SQLiteAsyncConnection>(_ =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return new SQLiteAsyncConnection(Path.Combine(appData, "ShinyWonderland.db"));
        });
        
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