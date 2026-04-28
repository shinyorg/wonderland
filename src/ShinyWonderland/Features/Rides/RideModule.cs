using Shiny.Jobs;
using ShinyWonderland.Delegates;

namespace ShinyWonderland.Features.Rides;

public class RideModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddJob(
            typeof(RideTimeJob),
            requiredNetwork: InternetAccess.Any,
            runInForeground: true
        );
    }

    public void Use(IPlatformApplication app)
    {
    }
}