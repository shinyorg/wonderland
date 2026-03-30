namespace ShinyWonderland.UITests;

public abstract class SettingsPageTests : PlatformTestBase
{
    protected SettingsPageTests(PlatformFixture fixture) : base(fixture) { }

    async Task NavigateToSettings()
    {
        await Driver.Navigate("//main/settings");
        await Driver.WaitUntilExists("SettingsPage");
    }

    [Fact]
    public async Task Settings_PageLoads()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SettingsPage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task Settings_TableViewExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SettingsTableView");
        isVisible.ShouldBeTrue("Settings table view should be present");
    }

    [Fact]
    public async Task Settings_SortByNameRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByName");
        isVisible.ShouldBeTrue("Sort by Name radio cell should be present");
    }

    [Fact]
    public async Task Settings_SortByWaitTimeRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByWaitTime");
        isVisible.ShouldBeTrue("Sort by Wait Time radio cell should be present");
    }

    [Fact]
    public async Task Settings_SortByPaidWaitTimeRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByPaidWaitTime");
        isVisible.ShouldBeTrue("Sort by Paid Wait Time radio cell should be present");
    }

    [Fact]
    public async Task Settings_SortByDistanceRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByDistance");
        isVisible.ShouldBeTrue("Sort by Distance radio cell should be present");
    }

    [Fact]
    public async Task Settings_TapSortByWaitTime()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "SortByWaitTime");
        await Driver.Screenshot("settings-sort-wait-time.png");
    }

    [Fact]
    public async Task Settings_TapSortByName()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "SortByName");
        await Driver.Screenshot("settings-sort-name.png");
    }

    [Fact]
    public async Task Settings_NotificationSwitchesExist()
    {
        await NavigateToSettings();

        var rideTime = await Driver.IsElementVisible("RideTimeNotifications");
        rideTime.ShouldBeTrue("Ride Time Notifications switch should exist");

        var geofence = await Driver.IsElementVisible("GeofenceNotifications");
        geofence.ShouldBeTrue("Geofence Notifications switch should exist");

        var drink = await Driver.IsElementVisible("DrinkNotifications");
        drink.ShouldBeTrue("Drink Notifications switch should exist");

        var meal = await Driver.IsElementVisible("MealNotifications");
        meal.ShouldBeTrue("Meal Notifications switch should exist");
    }

    [Fact]
    public async Task Settings_DisplaySwitchesExist()
    {
        await NavigateToSettings();

        var timedOnly = await Driver.IsElementVisible("ShowTimedOnly");
        timedOnly.ShouldBeTrue("Show Timed Only switch should exist");

        var openOnly = await Driver.IsElementVisible("ShowOpenOnly");
        openOnly.ShouldBeTrue("Show Open Only switch should exist");
    }

    [Fact]
    public async Task Settings_ToggleRideTimeNotifications()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "RideTimeNotifications");
        await Driver.Screenshot("settings-ride-notification-toggled.png");
    }

    [Fact]
    public async Task Settings_ToggleShowTimedOnly()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "ShowTimedOnly");
        await Driver.Screenshot("settings-timed-only-toggled.png");
    }

    [Fact]
    public async Task Settings_ToggleShowOpenOnly()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "ShowOpenOnly");
        await Driver.Screenshot("settings-open-only-toggled.png");
    }

    [Fact]
    public async Task Settings_VersionLabelExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("VersionLabel");
        isVisible.ShouldBeTrue("Version label should be present in About section");
    }

    [Fact]
    public async Task Settings_Screenshot()
    {
        await NavigateToSettings();

        await Driver.Screenshot("settings.png");
    }
}
