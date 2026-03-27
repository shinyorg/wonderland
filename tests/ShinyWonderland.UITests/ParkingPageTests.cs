namespace ShinyWonderland.UITests;

[Collection("App")]
public class ParkingPageTests
{
    readonly MauiDevFlowDriver driver;

    public ParkingPageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    async Task NavigateToParking()
    {
        await driver.Navigate("//main/parking");
        await driver.WaitUntilExists("ParkingPage");
    }

    [Fact]
    public async Task Parking_PageLoads()
    {
        await NavigateToParking();

        var isVisible = await driver.IsElementVisible("ParkingPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Parking_MapExists()
    {
        await NavigateToParking();

        var isVisible = await driver.IsElementVisible("ParkingMap");
        isVisible.ShouldBeTrue("Parking map should be visible");
    }

    [Fact]
    public async Task Parking_ToggleButtonExists()
    {
        await NavigateToParking();

        var isVisible = await driver.IsElementVisible("ToggleParkingButton");
        isVisible.ShouldBeTrue("Toggle parking button should be visible");
    }

    [Fact]
    public async Task Parking_TapSetParking()
    {
        await NavigateToParking();

        await driver.Tap(automationId: "ToggleParkingButton");
        await driver.Screenshot("parking-after-toggle.png");
    }

    [Fact]
    public async Task Parking_Screenshot()
    {
        await NavigateToParking();

        await driver.Screenshot("parking.png");
    }
}
