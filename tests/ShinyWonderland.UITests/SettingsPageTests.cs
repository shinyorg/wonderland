namespace ShinyWonderland.UITests;

public abstract class SettingsPageTests : PlatformTestBase
{
    async Task NavigateToSettings()
    {
        await Driver.Navigate("//main/settings");
        await Driver.WaitUntilExists("SettingsPage");
    }

    [Test]
    public async Task Settings_PageLoads()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SettingsPage");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Settings_TableViewExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SettingsTableView");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Settings_SortByNameRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByName");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Settings_SortByWaitTimeRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByWaitTime");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Settings_SortByPaidWaitTimeRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByPaidWaitTime");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Settings_SortByDistanceRadioExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("SortByDistance");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Settings_TapSortByWaitTime()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "SortByWaitTime");
        await Driver.Screenshot("settings-sort-wait-time.png");
    }

    [Test]
    public async Task Settings_TapSortByName()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "SortByName");
        await Driver.Screenshot("settings-sort-name.png");
    }

    [Test]
    public async Task Settings_NotificationSwitchesExist()
    {
        await NavigateToSettings();

        var rideTime = await Driver.IsElementVisible("RideTimeNotifications");
        await Assert.That(rideTime).IsTrue();

        var geofence = await Driver.IsElementVisible("GeofenceNotifications");
        await Assert.That(geofence).IsTrue();

        var drink = await Driver.IsElementVisible("DrinkNotifications");
        await Assert.That(drink).IsTrue();

        var meal = await Driver.IsElementVisible("MealNotifications");
        await Assert.That(meal).IsTrue();
    }

    [Test]
    public async Task Settings_DisplaySwitchesExist()
    {
        await NavigateToSettings();

        var timedOnly = await Driver.IsElementVisible("ShowTimedOnly");
        await Assert.That(timedOnly).IsTrue();

        var openOnly = await Driver.IsElementVisible("ShowOpenOnly");
        await Assert.That(openOnly).IsTrue();
    }

    [Test]
    public async Task Settings_ToggleRideTimeNotifications()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "RideTimeNotifications");
        await Driver.Screenshot("settings-ride-notification-toggled.png");
    }

    [Test]
    public async Task Settings_ToggleShowTimedOnly()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "ShowTimedOnly");
        await Driver.Screenshot("settings-timed-only-toggled.png");
    }

    [Test]
    public async Task Settings_ToggleShowOpenOnly()
    {
        await NavigateToSettings();

        await Driver.Tap(automationId: "ShowOpenOnly");
        await Driver.Screenshot("settings-open-only-toggled.png");
    }

    [Test]
    public async Task Settings_VersionLabelExists()
    {
        await NavigateToSettings();

        var isVisible = await Driver.IsElementVisible("VersionLabel");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task Settings_Screenshot()
    {
        await NavigateToSettings();

        await Driver.Screenshot("settings.png");
    }
}
