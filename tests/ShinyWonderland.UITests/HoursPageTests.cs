namespace ShinyWonderland.UITests;

[Collection("App")]
public class HoursPageTests
{
    readonly MauiDevFlowDriver driver;

    public HoursPageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    async Task NavigateToHours()
    {
        await driver.Navigate("//main/hours");
        await driver.WaitUntilExists("HoursPage");
    }

    [Fact]
    public async Task Hours_PageLoads()
    {
        await NavigateToHours();

        var isVisible = await driver.IsElementVisible("HoursPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Hours_CollectionViewExists()
    {
        await NavigateToHours();

        var isVisible = await driver.IsElementVisible("HoursCollectionView");
        isVisible.ShouldBeTrue("Hours collection view should be present");
    }

    [Fact]
    public async Task Hours_Screenshot()
    {
        await NavigateToHours();

        await driver.Screenshot("hours.png");
    }
}
