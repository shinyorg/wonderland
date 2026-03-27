namespace ShinyWonderland.UITests;

[Collection("App")]
public class StartupPageTests
{
    readonly MauiDevFlowDriver driver;

    public StartupPageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    [Fact]
    public async Task Startup_NavigatesToMainTabBar()
    {
        // By the time AppFixture completes, startup should have already
        // navigated to the main tab bar with RideTimesPage visible.
        var isVisible = await driver.IsElementVisible("RideTimesPage");
        isVisible.ShouldBeTrue("App should navigate from startup to main tab bar");
    }

    [Fact]
    public async Task Startup_StartupPageIsNoLongerVisible()
    {
        var isVisible = await driver.IsElementVisible("StartupPage");
        isVisible.ShouldBeFalse("Startup page should not be visible after navigation");
    }
}
