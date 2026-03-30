namespace ShinyWonderland.UITests;

public abstract class RideTimesPageTests : PlatformTestBase
{
    protected RideTimesPageTests(PlatformFixture fixture) : base(fixture) { }

    async Task NavigateToRideTimes()
    {
        await Driver.Navigate("//main/ridetimes");
        await Driver.WaitUntilExists("RideTimesPage");
    }

    [Fact]
    public async Task RideTimes_PageLoads()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("RideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task RideTimes_HasDataTimestamp()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("DataTimestampLabel");
        isVisible.ShouldBeTrue("Data timestamp label should be visible");
    }

    [Fact]
    public async Task RideTimes_CollectionViewExists()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("RidesCollectionView");
        isVisible.ShouldBeTrue("Rides collection view should be present");
    }

    [Fact]
    public async Task RideTimes_HasHistoryToolbarButton()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("HistoryToolbarButton");
        isVisible.ShouldBeTrue("History toolbar button should be visible");
    }

    [Fact]
    public async Task RideTimes_RefreshViewExists()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("RideTimesRefreshView");
        isVisible.ShouldBeTrue("RefreshView should be present for pull-to-refresh");
    }

    [Fact]
    public async Task RideTimes_HistoryButton_NavigatesToHistory()
    {
        await NavigateToRideTimes();

        await Driver.Tap(automationId: "HistoryToolbarButton");
        await Driver.WaitUntilExists("RideHistoryPage", timeoutSeconds: 10);

        var isVisible = await Driver.IsElementVisible("RideHistoryPage");
        isVisible.ShouldBeTrue("Should navigate to ride history page");

        // Navigate back
        await Driver.Navigate("//main/ridetimes");
        await Driver.WaitUntilExists("RideTimesPage");
    }

    [Fact]
    public async Task RideTimes_Screenshot()
    {
        await NavigateToRideTimes();

        // Visual verification screenshot
        await Driver.Screenshot("ride-times.png");
    }
}
