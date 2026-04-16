namespace ShinyWonderland.UITests;

public abstract class MapRideTimesPageTests : PlatformTestBase
{
    async Task NavigateToMapRideTimes()
    {
        await Driver.Navigate("//main/ridetimesmap");
        await Driver.WaitUntilExists("MapRideTimesPage");
    }

    [Test]
    public async Task MapRideTimes_PageLoads()
    {
        await NavigateToMapRideTimes();

        var isVisible = await Driver.IsElementVisible("MapRideTimesPage");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task MapRideTimes_MapIsPresent()
    {
        await NavigateToMapRideTimes();

        var isVisible = await Driver.IsElementVisible("RidesMap");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task MapRideTimes_Screenshot()
    {
        await NavigateToMapRideTimes();

        await Driver.Screenshot("map-ride-times.png");
    }
}
