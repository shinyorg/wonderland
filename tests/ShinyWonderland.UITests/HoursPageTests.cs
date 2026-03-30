namespace ShinyWonderland.UITests;

public abstract class HoursPageTests : PlatformTestBase
{
    protected HoursPageTests(PlatformFixture fixture) : base(fixture) { }

    async Task NavigateToHours()
    {
        await Driver.Navigate("//main/hours");
        await Driver.WaitUntilExists("HoursPage");
    }

    [Fact]
    public async Task Hours_PageLoads()
    {
        await NavigateToHours();

        var isVisible = await Driver.IsElementVisible("HoursPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Hours_CollectionViewExists()
    {
        await NavigateToHours();

        var isVisible = await Driver.IsElementVisible("HoursCollectionView");
        isVisible.ShouldBeTrue("Hours collection view should be present");
    }

    [Fact]
    public async Task Hours_Screenshot()
    {
        await NavigateToHours();

        await Driver.Screenshot("hours.png");
    }
}
