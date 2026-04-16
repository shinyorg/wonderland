namespace ShinyWonderland.UITests;

public abstract class RideTimesPageTests : PlatformTestBase
{
    async Task NavigateToRideTimes()
    {
        await Driver.Navigate("//main/ridetimes");
        await Driver.WaitUntilExists("RideTimesPage");
    }

    [Test]
    public async Task RideTimes_PageLoads()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("RideTimesPage");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task RideTimes_HasDataTimestamp()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("DataTimestampLabel");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task RideTimes_CollectionViewExists()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("RidesCollectionView");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task RideTimes_HasHistoryToolbarButton()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("HistoryToolbarButton");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task RideTimes_RefreshViewExists()
    {
        await NavigateToRideTimes();

        var isVisible = await Driver.IsElementVisible("RideTimesRefreshView");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task RideTimes_HistoryButton_NavigatesToHistory()
    {
        await NavigateToRideTimes();

        await Driver.Tap(automationId: "HistoryToolbarButton");
        await Driver.WaitUntilExists("RideHistoryPage", timeoutSeconds: 10);

        var isVisible = await Driver.IsElementVisible("RideHistoryPage");
        await Assert.That(isVisible).IsTrue();

        // Navigate back
        await Driver.Navigate("//main/ridetimes");
        await Driver.WaitUntilExists("RideTimesPage");
    }

    [Test]
    public async Task RideTimes_Screenshot()
    {
        await NavigateToRideTimes();

        // Visual verification screenshot
        await Driver.Screenshot("ride-times.png");
    }
}
