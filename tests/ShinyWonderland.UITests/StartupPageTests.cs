namespace ShinyWonderland.UITests;

public abstract class StartupPageTests : PlatformTestBase
{
    protected StartupPageTests(PlatformFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Startup_NavigatesToMainTabBar()
    {
        // By the time AppFixture completes, startup should have already
        // navigated to the main tab bar with RideTimesPage visible.
        var isVisible = await Driver.IsElementVisible("RideTimesPage");
        isVisible.ShouldBeTrue("App should navigate from startup to main tab bar");
    }

    [Fact]
    public async Task Startup_StartupPageIsNoLongerVisible()
    {
        var isVisible = await Driver.IsElementVisible("StartupPage");
        isVisible.ShouldBeFalse("Startup page should not be visible after navigation");
    }
}
