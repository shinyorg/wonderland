namespace ShinyWonderland.UITests;

public abstract class MapRideTimesPageTests : PlatformTestBase
{
    protected MapRideTimesPageTests(PlatformFixture fixture) : base(fixture) { }

    async Task NavigateToMapRideTimes()
    {
        await Driver.Navigate("//main/ridetimesmap");
        await Driver.WaitUntilExists("MapRideTimesPage");
    }

    [Fact]
    public async Task MapRideTimes_PageLoads()
    {
        await NavigateToMapRideTimes();

        var isVisible = await Driver.IsElementVisible("MapRideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task MapRideTimes_MapIsPresent()
    {
        await NavigateToMapRideTimes();

        var isVisible = await Driver.IsElementVisible("RidesMap");
        isVisible.ShouldBeTrue("Map control should be visible on the map ride times page");
    }

    [Fact]
    public async Task MapRideTimes_Screenshot()
    {
        await NavigateToMapRideTimes();

        await Driver.Screenshot("map-ride-times.png");
    }
}
