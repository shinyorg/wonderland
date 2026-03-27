namespace ShinyWonderland.UITests;

[Collection("App")]
public class RideTimesPageTests
{
    readonly MauiDevFlowDriver driver;

    public RideTimesPageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    async Task NavigateToRideTimes()
    {
        await driver.Navigate("//main/ridetimes");
        await driver.WaitUntilExists("RideTimesPage");
    }

    [Fact]
    public async Task RideTimes_PageLoads()
    {
        await NavigateToRideTimes();

        var isVisible = await driver.IsElementVisible("RideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task RideTimes_HasDataTimestamp()
    {
        await NavigateToRideTimes();

        var isVisible = await driver.IsElementVisible("DataTimestampLabel");
        isVisible.ShouldBeTrue("Data timestamp label should be visible");
    }

    [Fact]
    public async Task RideTimes_CollectionViewExists()
    {
        await NavigateToRideTimes();

        var isVisible = await driver.IsElementVisible("RidesCollectionView");
        isVisible.ShouldBeTrue("Rides collection view should be present");
    }

    [Fact]
    public async Task RideTimes_HasHistoryToolbarButton()
    {
        await NavigateToRideTimes();

        var isVisible = await driver.IsElementVisible("HistoryToolbarButton");
        isVisible.ShouldBeTrue("History toolbar button should be visible");
    }

    [Fact]
    public async Task RideTimes_RefreshViewExists()
    {
        await NavigateToRideTimes();

        var isVisible = await driver.IsElementVisible("RideTimesRefreshView");
        isVisible.ShouldBeTrue("RefreshView should be present for pull-to-refresh");
    }

    [Fact]
    public async Task RideTimes_HistoryButton_NavigatesToHistory()
    {
        await NavigateToRideTimes();

        await driver.Tap(automationId: "HistoryToolbarButton");
        await driver.WaitUntilExists("RideHistoryPage", timeoutSeconds: 10);

        var isVisible = await driver.IsElementVisible("RideHistoryPage");
        isVisible.ShouldBeTrue("Should navigate to ride history page");

        // Navigate back
        await driver.Navigate("//main/ridetimes");
        await driver.WaitUntilExists("RideTimesPage");
    }

    [Fact]
    public async Task RideTimes_Screenshot()
    {
        await NavigateToRideTimes();

        // Visual verification screenshot
        await driver.Screenshot("ride-times.png");
    }
}
