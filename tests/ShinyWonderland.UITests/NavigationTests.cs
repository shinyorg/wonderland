namespace ShinyWonderland.UITests;

public abstract class NavigationTests : PlatformTestBase
{
    protected NavigationTests(PlatformFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Navigate_ToRideTimesTab()
    {
        await Driver.Navigate("//main/ridetimes");
        await Driver.WaitUntilExists("RideTimesPage");

        var isVisible = await Driver.IsElementVisible("RideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToMapRideTimesTab()
    {
        await Driver.Navigate("//main/ridetimesmap");
        await Driver.WaitUntilExists("MapRideTimesPage");

        var isVisible = await Driver.IsElementVisible("MapRideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToSettingsTab()
    {
        await Driver.Navigate("//main/settings");
        await Driver.WaitUntilExists("SettingsPage");

        var isVisible = await Driver.IsElementVisible("SettingsPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToParkingTab()
    {
        await Driver.Navigate("//main/parking");
        await Driver.WaitUntilExists("ParkingPage");

        var isVisible = await Driver.IsElementVisible("ParkingPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToMealTimesTab()
    {
        await Driver.Navigate("//main/mealtimes");
        await Driver.WaitUntilExists("MealTimePage");

        var isVisible = await Driver.IsElementVisible("MealTimePage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToHoursTab()
    {
        await Driver.Navigate("//main/hours");
        await Driver.WaitUntilExists("HoursPage");

        var isVisible = await Driver.IsElementVisible("HoursPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_AllTabs_Sequentially()
    {
        var tabs = new[]
        {
            ("//main/ridetimes", "RideTimesPage"),
            ("//main/settings", "SettingsPage"),
            ("//main/parking", "ParkingPage"),
            ("//main/mealtimes", "MealTimePage"),
            ("//main/hours", "HoursPage"),
            ("//main/ridetimesmap", "MapRideTimesPage"),
            ("//main/ridetimes", "RideTimesPage")
        };

        foreach (var (route, pageId) in tabs)
        {
            await Driver.Navigate(route);
            await Driver.WaitUntilExists(pageId);

            var isVisible = await Driver.IsElementVisible(pageId);
            isVisible.ShouldBeTrue($"Page {pageId} should be visible after navigating to {route}");
        }
    }
}
