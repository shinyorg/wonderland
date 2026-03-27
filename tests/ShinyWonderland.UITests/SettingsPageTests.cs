namespace ShinyWonderland.UITests;

[Collection("App")]
public class SettingsPageTests
{
    readonly MauiDevFlowDriver driver;

    public SettingsPageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    async Task NavigateToSettings()
    {
        await driver.Navigate("//main/settings");
        await driver.WaitUntilExists("SettingsPage");
    }

    [Fact]
    public async Task Settings_PageLoads()
    {
        await NavigateToSettings();

        var isVisible = await driver.IsElementVisible("SettingsPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Settings_TableViewExists()
    {
        await NavigateToSettings();

        var isVisible = await driver.IsElementVisible("SettingsTableView");
        isVisible.ShouldBeTrue("Settings table view should be present");
    }

    [Fact]
    public async Task Settings_SortByNameRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await driver.IsElementVisible("SortByName");
        isVisible.ShouldBeTrue("Sort by Name radio cell should be present");
    }

    [Fact]
    public async Task Settings_SortByWaitTimeRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await driver.IsElementVisible("SortByWaitTime");
        isVisible.ShouldBeTrue("Sort by Wait Time radio cell should be present");
    }

    [Fact]
    public async Task Settings_SortByPaidWaitTimeRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await driver.IsElementVisible("SortByPaidWaitTime");
        isVisible.ShouldBeTrue("Sort by Paid Wait Time radio cell should be present");
    }

    [Fact]
    public async Task Settings_SortByDistanceRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await driver.IsElementVisible("SortByDistance");
        isVisible.ShouldBeTrue("Sort by Distance radio cell should be present");
    }

    [Fact]
    public async Task Settings_TapSortByWaitTime()
    {
        await NavigateToSettings();

        await driver.Tap(automationId: "SortByWaitTime");
        await driver.Screenshot("settings-sort-wait-time.png");
    }

    [Fact]
    public async Task Settings_TapSortByName()
    {
        await NavigateToSettings();

        await driver.Tap(automationId: "SortByName");
        await driver.Screenshot("settings-sort-name.png");
    }

    [Fact]
    public async Task Settings_NotificationSwitchesExist()
    {
        await NavigateToSettings();

        var rideTime = await driver.IsElementVisible("RideTimeNotifications");
        rideTime.ShouldBeTrue("Ride Time Notifications switch should exist");

        var geofence = await driver.IsElementVisible("GeofenceNotifications");
        geofence.ShouldBeTrue("Geofence Notifications switch should exist");

        var drink = await driver.IsElementVisible("DrinkNotifications");
        drink.ShouldBeTrue("Drink Notifications switch should exist");

        var meal = await driver.IsElementVisible("MealNotifications");
        meal.ShouldBeTrue("Meal Notifications switch should exist");
    }

    [Fact]
    public async Task Settings_DisplaySwitchesExist()
    {
        await NavigateToSettings();

        var timedOnly = await driver.IsElementVisible("ShowTimedOnly");
        timedOnly.ShouldBeTrue("Show Timed Only switch should exist");

        var openOnly = await driver.IsElementVisible("ShowOpenOnly");
        openOnly.ShouldBeTrue("Show Open Only switch should exist");
    }

    [Fact]
    public async Task Settings_ToggleRideTimeNotifications()
    {
        await NavigateToSettings();

        await driver.Tap(automationId: "RideTimeNotifications");
        await driver.Screenshot("settings-ride-notification-toggled.png");
    }

    [Fact]
    public async Task Settings_ToggleShowTimedOnly()
    {
        await NavigateToSettings();

        await driver.Tap(automationId: "ShowTimedOnly");
        await driver.Screenshot("settings-timed-only-toggled.png");
    }

    [Fact]
    public async Task Settings_ToggleShowOpenOnly()
    {
        await NavigateToSettings();

        await driver.Tap(automationId: "ShowOpenOnly");
        await driver.Screenshot("settings-open-only-toggled.png");
    }

    [Fact]
    public async Task Settings_VersionLabelExists()
    {
        await NavigateToSettings();

        var isVisible = await driver.IsElementVisible("VersionLabel");
        isVisible.ShouldBeTrue("Version label should be present in About section");
    }

    [Fact]
    public async Task Settings_Screenshot()
    {
        await NavigateToSettings();

        await driver.Screenshot("settings.png");
    }
}
