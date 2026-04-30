#if PLATFORM
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using MauiDevFlow.Agent;
using Microsoft.Extensions.Configuration;
using ShinyWonderland.Delegates;

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
            .UseMauiCommunityToolkit()
            .UseMauiMaps()
            .UseShiny()
            .UseShinyControls()
            .UseShinyShell(x => x
                .AddGeneratedMaps()
                .UseUxDiversDialogs()
                .AddAiTools()
            )
            .AddShinyMediator(
                x => x
                    .AddMediatorRegistry()
                    .AddGeneratedAITools()
                    .AddGeneratedOpenApiClient()
                    .AddMauiPersistentCache()
                    .AddConnectivityBroadcaster()
                    .UseSentry(),
                false
            )
            .AddInfrastructureModules(
                new AIModule(),
                new MealTimesModule(),
                new RideModule(),
                new ParkingModule()
            )
#if RELEASE
            .UseSentry(x => x.Dsn = builder.Configuration["SentryDsn"]!)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services.Configure<ParkOptions>(builder.Configuration.GetSection("Park"));
        builder.Services.AddStronglyTypedLocalizations();
        builder.Services.AddGeneratedServices();
        builder.Services.AddShinyService<AppSettings>();
        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton(TextToSpeech.Default);
        builder.Services.AddSingleton(SpeechToText.Default);
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddDatabase();
        builder.Services.AddNotifications();
        builder.Services.AddGps<MyGpsDelegate>();
        
        var app = builder.Build();
        return app;
    }
}
#endif