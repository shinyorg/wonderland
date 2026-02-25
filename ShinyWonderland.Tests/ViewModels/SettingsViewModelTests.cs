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

    [Fact]
    public void Sorts_ShouldContainAllOptions()
    {
        // Assert
        viewModel.Sorts.ShouldContain("Name");
        viewModel.Sorts.ShouldContain("Wait Time");
        viewModel.Sorts.ShouldContain("Paid Wait Time");
        viewModel.Sorts.ShouldContain("Distance");
        viewModel.Sorts.Length.ShouldBe(4);
    }

    [Theory]
    [InlineData(0, RideOrder.Name)]
    [InlineData(1, RideOrder.WaitTime)]
    [InlineData(2, RideOrder.PaidWaitTime)]
    [InlineData(3, RideOrder.Distance)]
    public void SortByIndex_ShouldUpdateAppSettings(int index, RideOrder expectedOrder)
    {
        // Act
        viewModel.SortByIndex = index;

        // Assert
        appSettings.Ordering.ShouldBe(expectedOrder);
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
    public void SortByIndex_InitialValue_ShouldMapFromAppSettings()
    {
        // Arrange
        var settings = new AppSettings { Ordering = RideOrder.WaitTime };
        var vm = new SettingsViewModel(settings);

        // Assert
        vm.SortByIndex.ShouldBe(1);
    }
}