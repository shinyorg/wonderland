namespace ShinyWonderland.UITests;

[Collection("App")]
public class NavigationTests
{
    readonly MauiDevFlowDriver driver;

    public NavigationTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    [Fact]
    public async Task Navigate_ToRideTimesTab()
    {
        await driver.Navigate("//main/ridetimes");
        await driver.WaitUntilExists("RideTimesPage");

        var isVisible = await driver.IsElementVisible("RideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToMapRideTimesTab()
    {
        await driver.Navigate("//main/ridetimesmap");
        await driver.WaitUntilExists("MapRideTimesPage");

        var isVisible = await driver.IsElementVisible("MapRideTimesPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToSettingsTab()
    {
        await driver.Navigate("//main/settings");
        await driver.WaitUntilExists("SettingsPage");

        var isVisible = await driver.IsElementVisible("SettingsPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToParkingTab()
    {
        await driver.Navigate("//main/parking");
        await driver.WaitUntilExists("ParkingPage");

        var isVisible = await driver.IsElementVisible("ParkingPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToMealTimesTab()
    {
        await driver.Navigate("//main/mealtimes");
        await driver.WaitUntilExists("MealTimePage");

        var isVisible = await driver.IsElementVisible("MealTimePage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Navigate_ToHoursTab()
    {
        await driver.Navigate("//main/hours");
        await driver.WaitUntilExists("HoursPage");

        var isVisible = await driver.IsElementVisible("HoursPage");
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
            await driver.Navigate(route);
            await driver.WaitUntilExists(pageId);

            var isVisible = await driver.IsElementVisible(pageId);
            isVisible.ShouldBeTrue($"Page {pageId} should be visible after navigating to {route}");
        }
    }
}
