namespace ShinyWonderland.UITests;

public abstract class RideHistoryPageTests : PlatformTestBase
{
    async Task NavigateToRideHistory()
    {
        // Navigate to ride times first, then tap history button
        await Driver.Navigate("//main/ridetimes");
        await Driver.WaitUntilExists("RideTimesPage");
        await Driver.Tap(automationId: "HistoryToolbarButton");
        await Driver.WaitUntilExists("RideHistoryPage", timeoutSeconds: 10);
    }

    [Test]
    public async Task RideHistory_PageLoads()
    {
        await NavigateToRideHistory();

        var isVisible = await Driver.IsElementVisible("RideHistoryPage");
        await Assert.That(isVisible).IsTrue();

        // Navigate back
        await Driver.Navigate("//main/ridetimes");
    }

    [Test]
    public async Task RideHistory_CollectionViewExists()
    {
        await NavigateToRideHistory();

        var isVisible = await Driver.IsElementVisible("RideHistoryCollectionView");
        await Assert.That(isVisible).IsTrue();

        // Navigate back
        await Driver.Navigate("//main/ridetimes");
    }

    [Test]
    public async Task RideHistory_Screenshot()
    {
        await NavigateToRideHistory();

        await Driver.Screenshot("ride-history.png");

        // Navigate back
        await Driver.Navigate("//main/ridetimes");
    }
}
