namespace ShinyWonderland.Tests.ViewModels;

public class SettingsViewModelTests
{
    readonly AppSettings appSettings;
    readonly SettingsViewModel viewModel;

    public SettingsViewModelTests()
    {
        appSettings = new AppSettings();
        viewModel = new SettingsViewModel(appSettings);
    }

    [Test]
    public async Task AppVersion_ShouldNotBeEmpty()
    {
        await Assert.That(viewModel.AppVersion).IsNotNull();
        await Assert.That(viewModel.AppVersion).IsNotEmpty();
    }

    [Test]
    [Arguments(RideOrder.Name)]
    [Arguments(RideOrder.WaitTime)]
    [Arguments(RideOrder.PaidWaitTime)]
    [Arguments(RideOrder.Distance)]
    public async Task Ordering_ShouldUpdateAppSettings(RideOrder order)
    {
        viewModel.Ordering = order;
        await Assert.That(appSettings.Ordering).IsEqualTo(order);
    }

    [Test]
    public async Task ShowOpenOnly_ShouldUpdateAppSettings()
    {
        viewModel.ShowOpenOnly = false;
        await Assert.That(appSettings.ShowOpenOnly).IsFalse();

        viewModel.ShowOpenOnly = true;
        await Assert.That(appSettings.ShowOpenOnly).IsTrue();
    }

    [Test]
    public async Task ShowTimedOnly_ShouldUpdateAppSettings()
    {
        viewModel.ShowTimedOnly = false;
        await Assert.That(appSettings.ShowTimedOnly).IsFalse();
    }

    [Test]
    public async Task EnableTimeRideNotifications_ShouldUpdateAppSettings()
    {
        viewModel.EnableTimeRideNotifications = false;
        await Assert.That(appSettings.EnableTimeRideNotifications).IsFalse();
    }

    [Test]
    public async Task EnableDrinkNotifications_ShouldUpdateAppSettings()
    {
        viewModel.EnableDrinkNotifications = false;
        await Assert.That(appSettings.EnableDrinkNotifications).IsFalse();
    }

    [Test]
    public async Task EnableMealNotifications_ShouldUpdateAppSettings()
    {
        viewModel.EnableMealNotifications = false;
        await Assert.That(appSettings.EnableMealNotifications).IsFalse();
    }

    [Test]
    public async Task EnableGeofenceNotifications_ShouldUpdateAppSettings()
    {
        viewModel.EnableGeofenceNotifications = false;
        await Assert.That(appSettings.EnableGeofenceNotifications).IsFalse();
    }

    [Test]
    public async Task InitialValues_ShouldMatchAppSettingsDefaults()
    {
        var defaultSettings = new AppSettings();
        var vm = new SettingsViewModel(defaultSettings);

        await Assert.That(vm.ShowOpenOnly).IsEqualTo(defaultSettings.ShowOpenOnly);
        await Assert.That(vm.ShowTimedOnly).IsEqualTo(defaultSettings.ShowTimedOnly);
        await Assert.That(vm.EnableTimeRideNotifications).IsEqualTo(defaultSettings.EnableTimeRideNotifications);
        await Assert.That(vm.EnableDrinkNotifications).IsEqualTo(defaultSettings.EnableDrinkNotifications);
        await Assert.That(vm.EnableMealNotifications).IsEqualTo(defaultSettings.EnableMealNotifications);
        await Assert.That(vm.EnableGeofenceNotifications).IsEqualTo(defaultSettings.EnableGeofenceNotifications);
    }

    [Test]
    public async Task Ordering_InitialValue_ShouldMatchAppSettings()
    {
        var settings = new AppSettings { Ordering = RideOrder.WaitTime };
        var vm = new SettingsViewModel(settings);
        await Assert.That(vm.Ordering).IsEqualTo(RideOrder.WaitTime);
    }
}
