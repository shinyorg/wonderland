namespace ShinyWonderland.UITests;

public abstract class StartupPageTests : PlatformTestBase
{
    [Test]
    public async Task Startup_NavigatesToMainTabBar()
    {
        // By the time AppFixture completes, startup should have already
        // navigated to the main tab bar with RideTimesPage visible.
        var isVisible = await Driver.IsElementVisible("RideTimesPage");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Startup_StartupPageIsNoLongerVisible()
    {
        var isVisible = await Driver.IsElementVisible("StartupPage");
        await Assert.That(isVisible).IsFalse();
    }
}
