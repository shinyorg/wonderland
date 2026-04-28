using ShinyWonderland.Delegates;

namespace ShinyWonderland.Features.MealTimes;

public class MealTimesModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddJob(
            typeof(MealTimeJob),
            runInForeground: true
        );
        builder.Services.Configure<MealTimeOptions>(builder.Configuration.GetSection("MealPass"));
    }

    public void Use(IPlatformApplication app)
    {
    }
}
