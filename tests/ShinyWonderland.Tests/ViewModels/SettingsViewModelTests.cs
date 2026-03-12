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

    [Fact]
    public void AppVersion_ShouldNotBeEmpty()
    {
        // Assert
        viewModel.AppVersion.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(RideOrder.Name)]
    [InlineData(RideOrder.WaitTime)]
    [InlineData(RideOrder.PaidWaitTime)]
    [InlineData(RideOrder.Distance)]
    public void Ordering_ShouldUpdateAppSettings(RideOrder order)
    {
        // Act
        viewModel.Ordering = order;

        // Assert
        appSettings.Ordering.ShouldBe(order);
    }

    [Fact]
    public void ShowOpenOnly_ShouldUpdateAppSettings()
    {
        // Act
        viewModel.ShowOpenOnly = false;

        // Assert
        appSettings.ShowOpenOnly.ShouldBeFalse();

        // Act
        viewModel.ShowOpenOnly = true;

        // Assert
        appSettings.ShowOpenOnly.ShouldBeTrue();
    }

    [Fact]
    public void ShowTimedOnly_ShouldUpdateAppSettings()
    {
        // Act
        viewModel.ShowTimedOnly = false;

        // Assert
        appSettings.ShowTimedOnly.ShouldBeFalse();
    }

    [Fact]
    public void EnableTimeRideNotifications_ShouldUpdateAppSettings()
    {
        // Act
        viewModel.EnableTimeRideNotifications = false;

        // Assert
        appSettings.EnableTimeRideNotifications.ShouldBeFalse();
    }

    [Fact]
    public void EnableDrinkNotifications_ShouldUpdateAppSettings()
    {
        // Act
        viewModel.EnableDrinkNotifications = false;

        // Assert
        appSettings.EnableDrinkNotifications.ShouldBeFalse();
    }

    [Fact]
    public void EnableMealNotifications_ShouldUpdateAppSettings()
    {
        // Act
        viewModel.EnableMealNotifications = false;

        // Assert
        appSettings.EnableMealNotifications.ShouldBeFalse();
    }

    [Fact]
    public void EnableGeofenceNotifications_ShouldUpdateAppSettings()
    {
        // Act
        viewModel.EnableGeofenceNotifications = false;

        // Assert
        appSettings.EnableGeofenceNotifications.ShouldBeFalse();
    }

    [Fact]
    public void InitialValues_ShouldMatchAppSettingsDefaults()
    {
        // Arrange
        var defaultSettings = new AppSettings();
        var vm = new SettingsViewModel(defaultSettings);

        // Assert
        vm.ShowOpenOnly.ShouldBe(defaultSettings.ShowOpenOnly);
        vm.ShowTimedOnly.ShouldBe(defaultSettings.ShowTimedOnly);
        vm.EnableTimeRideNotifications.ShouldBe(defaultSettings.EnableTimeRideNotifications);
        vm.EnableDrinkNotifications.ShouldBe(defaultSettings.EnableDrinkNotifications);
        vm.EnableMealNotifications.ShouldBe(defaultSettings.EnableMealNotifications);
        vm.EnableGeofenceNotifications.ShouldBe(defaultSettings.EnableGeofenceNotifications);
    }

    [Fact]
    public void Ordering_InitialValue_ShouldMatchAppSettings()
    {
        // Arrange
        var settings = new AppSettings { Ordering = RideOrder.WaitTime };
        var vm = new SettingsViewModel(settings);

        // Assert
        vm.Ordering.ShouldBe(RideOrder.WaitTime);
    }
}