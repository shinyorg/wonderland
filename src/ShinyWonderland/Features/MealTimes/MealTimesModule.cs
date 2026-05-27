using Shiny.Jobs;

namespace ShinyWonderland.Features.MealTimes;

public class MealTimesModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddJob<MealTimeJob>(x => x.WithForeground().WithInternet(InternetAccess.Any));
        builder.Services.Configure<MealTimeOptions>(builder.Configuration.GetSection("MealPass"));
    }

    public void Use(IPlatformApplication app)
    {
    }
}
