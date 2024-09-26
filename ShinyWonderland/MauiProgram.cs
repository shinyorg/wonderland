﻿using Polly;
using Polly.Retry;

namespace ShinyWonderland;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UsePrism(
                new DryIocContainerExtension(),
                prism => prism.CreateWindow("NavigationPage/MainPage")
            )
            .UseShiny()
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
        builder.Services.AddConnectivity();
        builder.Services.AddBattery();
        builder.Services.AddShinyMediator(x => x
            .AddMemoryCaching()
            .AddDataAnnotations()
            .AddResiliencyMiddleware(
                ("Default", pipeline =>
                {
                    pipeline.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        MaxDelay = TimeSpan.FromSeconds(1.0),
                    });
                    pipeline.AddTimeout(TimeSpan.FromSeconds(5));
                })
            )
            .UseMaui()
        );
        builder.Services.AddJob(typeof(ShinyWonderland.Delegates.MyJob));
        builder.Services.AddNotifications<ShinyWonderland.Delegates.MyLocalNotificationDelegate>();

        builder.Services.RegisterForNavigation<MainPage, MainViewModel>();
        var app = builder.Build();

        return app;
    }
}