namespace ShinyWonderland.UITests;

public abstract class ParkingPageTests : PlatformTestBase
{
    protected ParkingPageTests(PlatformFixture fixture) : base(fixture) { }

    async Task NavigateToParking()
    {
        await Driver.Navigate("//main/parking");
        await Driver.WaitUntilExists("ParkingPage");
    }

    [Fact]
    public async Task Parking_PageLoads()
    {
        await NavigateToParking();

        var isVisible = await Driver.IsElementVisible("ParkingPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Parking_MapExists()
    {
        await NavigateToParking();

        var isVisible = await Driver.IsElementVisible("ParkingMap");
        isVisible.ShouldBeTrue("Parking map should be visible");
    }

    [Fact]
    public async Task Parking_ToggleButtonExists()
    {
        await NavigateToParking();

        var isVisible = await Driver.IsElementVisible("ToggleParkingButton");
        isVisible.ShouldBeTrue("Toggle parking button should be visible");
    }

    [Fact]
    public async Task Parking_TapSetParking()
    {
        await NavigateToParking();

        await Driver.Tap(automationId: "ToggleParkingButton");
        await Driver.Screenshot("parking-after-toggle.png");
    }

    [Fact]
    public async Task Parking_Screenshot()
    {
        await NavigateToParking();

        await Driver.Screenshot("parking.png");
    }
}
