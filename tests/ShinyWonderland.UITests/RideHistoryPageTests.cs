namespace ShinyWonderland.UITests;

[Collection("App")]
public class RideHistoryPageTests
{
    readonly MauiDevFlowDriver driver;

    public RideHistoryPageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    async Task NavigateToRideHistory()
    {
        // Navigate to ride times first, then tap history button
        await driver.Navigate("//main/ridetimes");
        await driver.WaitUntilExists("RideTimesPage");
        await driver.Tap(automationId: "HistoryToolbarButton");
        await driver.WaitUntilExists("RideHistoryPage", timeoutSeconds: 10);
    }

    [Fact]
    public async Task RideHistory_PageLoads()
    {
        await NavigateToRideHistory();

        var isVisible = await driver.IsElementVisible("RideHistoryPage");
        isVisible.ShouldBeTrue();

        // Navigate back
        await driver.Navigate("//main/ridetimes");
    }

    [Fact]
    public async Task RideHistory_CollectionViewExists()
    {
        await NavigateToRideHistory();

        var isVisible = await driver.IsElementVisible("RideHistoryCollectionView");
        isVisible.ShouldBeTrue("Ride history collection view should be present");

        // Navigate back
        await driver.Navigate("//main/ridetimes");
    }

    [Fact]
    public async Task RideHistory_Screenshot()
    {
        await NavigateToRideHistory();

        await driver.Screenshot("ride-history.png");

        // Navigate back
        await driver.Navigate("//main/ridetimes");
    }
}
