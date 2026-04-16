namespace ShinyWonderland.UITests;

public abstract class HoursPageTests : PlatformTestBase
{
    async Task NavigateToHours()
    {
        await Driver.Navigate("//main/hours");
        await Driver.WaitUntilExists("HoursPage");
    }

    [Test]
    public async Task Hours_PageLoads()
    {
        await NavigateToHours();

        var isVisible = await Driver.IsElementVisible("HoursPage");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Hours_CollectionViewExists()
    {
        await NavigateToHours();

        var isVisible = await Driver.IsElementVisible("HoursCollectionView");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Hours_Screenshot()
    {
        await NavigateToHours();

        await Driver.Screenshot("hours.png");
    }
}
