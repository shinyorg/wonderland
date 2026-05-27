using Shiny.Jobs;
using ShinyWonderland.Delegates;

namespace ShinyWonderland.Features.Rides;

public class RideModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddJob<RideTimeJob>(x => x
            .WithForeground()
            .WithInternet(InternetAccess.Any)
        );
    }

    public void Use(IPlatformApplication app)
    {
    }
}