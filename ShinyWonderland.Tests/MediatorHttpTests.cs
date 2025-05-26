using Microsoft.Extensions.Configuration;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland.Tests;


public class MediatorHttpTests
{
    readonly IMediator mediator;
    
    //     "EntityId": "66f5d97a-a530-40bf-a712-a6317c96b06d",
    //     "Latitude": 43.843,
    //     "Longitude": -79.53896,
    
    public MediatorHttpTests()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationManager();
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Mediator:Http:*", "https://api.themeparks.wiki/v1" }
        });
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddDiscoveredMediatorHandlersFromShinyWonderland();
        services.AddShinyMediator(x => x.AddHttpClient(), false);
        var sp = services.BuildServiceProvider();
        this.mediator = sp.GetRequiredService<IMediator>();
    }
    
    
    [Fact]
    public async Task GetEntityChildrenHttpRequest_Test()
    {
        await this.mediator.Request(new GetEntityChildrenHttpRequest
        {
            EntityID = "66f5d97a-a530-40bf-a712-a6317c96b06d"
        });
    }
    
    
    [Fact]
    public async Task GetEntityLiveDataHttpRequest_Test()
    {
        await this.mediator.Request(new GetEntityLiveDataHttpRequest
        {
            EntityID = "66f5d97a-a530-40bf-a712-a6317c96b06d"
        });
    }
}