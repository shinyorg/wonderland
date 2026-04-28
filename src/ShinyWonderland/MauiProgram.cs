#if PLATFORM
using System.ClientModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
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
                    .UseSentry()
                    .UseMaui(false),
                false
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
        builder.Services.Configure<MealTimeOptions>(builder.Configuration.GetSection("MealPass"));
        builder.Services.AddWonderlandLocalization();
        
        builder.Services.AddGeneratedServices();
        builder.Services.AddShinyService<AppSettings>();

        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton(TextToSpeech.Default);
        builder.Services.AddSingleton(SpeechToText.Default);
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddChatClient(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var apiKey = config["GitHubCopilot:ApiKey"] ?? "";
            var model = config["GitHubCopilot:Model"] ?? "gpt-4o";

            var client = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.githubcopilot.com")
                }
            );
            return client.GetChatClient(model).AsIChatClient();
        });

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