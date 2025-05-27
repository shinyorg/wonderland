using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ShinyWonderland.Delegates;
using ShinyWonderland.Services;
using Xunit.Abstractions;

namespace ShinyWonderland.Tests;

public class MyJobTests(ITestOutputHelper output)
{
    [Fact]
    public void Test1()
    {
        var appSettings = new AppSettings
        {
            EnableNotifications = true
        };
        var parkOptions = new OptionsWrapper<ParkOptions>(new ParkOptions
        {
            Latitude = 0,
            Longitude = 0
        });
        var core = new CoreServices(
            null, // IMediator
            parkOptions,
            appSettings,
            null, // INavigator
            FakeTimeProvider.System,
            null, // gps manager
            null // notification manager
        );
        
        var job = new MyJob(
            new Logger<MyJob>(output),
            core
        );
        
        
    }
}