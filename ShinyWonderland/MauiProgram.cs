﻿using CommunityToolkit.Maui;

namespace ShinyWonderland;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
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
            .AddDataAnnotations()
            .AddPersistentCache()
            .UseMaui()
        );
        builder.Services.AddJob(typeof(ShinyWonderland.Delegates.MyJob));
        builder.Services.AddNotifications();

        builder.Services.RegisterForNavigation<MainPage, MainViewModel>();
        var app = builder.Build();

        return app;
    }
}