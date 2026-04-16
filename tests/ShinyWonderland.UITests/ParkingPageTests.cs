namespace ShinyWonderland.UITests;

public abstract class ParkingPageTests : PlatformTestBase
{
    async Task NavigateToParking()
    {
        await Driver.Navigate("//main/parking");
        await Driver.WaitUntilExists("ParkingPage");
    }

    [Test]
    public async Task Parking_PageLoads()
    {
        await NavigateToParking();

        var isVisible = await Driver.IsElementVisible("ParkingPage");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Parking_MapExists()
    {
        await NavigateToParking();

        var isVisible = await Driver.IsElementVisible("ParkingMap");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Parking_ToggleButtonExists()
    {
        await NavigateToParking();

        var isVisible = await Driver.IsElementVisible("ToggleParkingButton");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Parking_TapSetParking()
    {
        await NavigateToParking();

        await Driver.Tap(automationId: "ToggleParkingButton");
        await Driver.Screenshot("parking-after-toggle.png");
    }

    [Test]
    public async Task Parking_Screenshot()
    {
        await NavigateToParking();

        await Driver.Screenshot("parking.png");
    }
}
