namespace ShinyWonderland.UITests;

[Collection("App")]
public class MapRideTimesPageTests
{
    readonly MauiDevFlowDriver driver;

    public MapRideTimesPageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    async Task NavigateToMapRideTimes()
    {
        await driver.Navigate("//main/ridetimesmap");
        await driver.WaitUntilExists("MapRideTimesPage");
    }

    [Fact]
    public async Task MapRideTimes_PageLoads()
    {
        await NavigateToMapRideTimes();

        var isVisible = await driver.IsElementVisible("MapRideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task MapRideTimes_MapIsPresent()
    {
        await NavigateToMapRideTimes();

        var isVisible = await driver.IsElementVisible("RidesMap");
        isVisible.ShouldBeTrue("Map control should be visible on the map ride times page");
    }

    [Fact]
    public async Task MapRideTimes_Screenshot()
    {
        await NavigateToMapRideTimes();

        await driver.Screenshot("map-ride-times.png");
    }
}
