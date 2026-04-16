using Microsoft.Maui.Media;
using Shiny.Notifications;

namespace ShinyWonderland.Tests.ViewModels;

public class ParkingViewModelTests
{
    readonly AppSettings appSettings;
    readonly StringsLocalized localize;
    readonly ParkingViewModel viewModel;

    public ParkingViewModelTests()
    {
        localize = TestLocalization.Create(new Dictionary<string, string>
        {
            ["SetParking"] = "Set Parking Location",
            ["RemoveParking"] = "Remove Parking Location",
            ["PermissionDenied"] = "Permission Denied",
            ["OpenSettings"] = "Open Settings?",
            ["Reset"] = "Reset",
            ["ConfirmReset"] = "Are you sure you want to reset?",
            ["Error"] = "Error",
            ["NotCloseEnough"] = "Not close enough to park",
            ["ErrorRetrievingLocation"] = "Error retrieving location"
        });

        appSettings = new AppSettings();

        var parkOptions = new ParkOptions
        {
            Name = "Test Park",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000,
            MapStartZoomDistanceMeters = 500
        };

        var services = new CoreServices(
            new TestMediator(),
            Options.Create(parkOptions),
            appSettings,
            new TestNavigator(),
            new IDialogsImposter().Instance(),
            new FakeTimeProvider(DateTimeOffset.UtcNow),
            new IGpsManagerImposter().Instance(),
            localize,
            new INotificationManagerImposter().Instance()
        );

        viewModel = new ParkingViewModel(
            services,
            new IMediaPickerImposter().Instance(),
            new ILoggerImposter<ParkingViewModel>().Instance()
        );
    }

    [Test]
    public async Task CommandText_WhenNotParked_ShouldReturnSetParking()
    {
        await Assert.That(viewModel.ParkLocation).IsNull();
        await Assert.That(viewModel.CommandText).IsEqualTo("Set Parking Location");
    }

    [Test]
    public async Task CommandText_WhenParked_ShouldReturnRemoveParking()
    {
        viewModel.ParkLocation = new Position(33.8121, -117.9190);

        await Assert.That(viewModel.CommandText).IsEqualTo("Remove Parking Location");
    }

    [Test]
    public async Task IsParked_WhenNoParkLocation_ShouldBeFalse()
    {
        await Assert.That(viewModel.IsParked).IsFalse();
    }

    [Test]
    public async Task IsParked_WhenParkLocationSet_ShouldBeTrue()
    {
        viewModel.ParkLocation = new Position(33.8121, -117.9190);

        await Assert.That(viewModel.IsParked).IsTrue();
    }

    [Test]
    public async Task HasParkedImage_WhenNoImage_ShouldBeFalse()
    {
        await Assert.That(viewModel.HasParkedImage).IsFalse();
    }

    [Test]
    public async Task HasParkedImage_WhenImageSet_ShouldBeTrue()
    {
        viewModel.ImageUri = "file://some/path.png";

        await Assert.That(viewModel.HasParkedImage).IsTrue();
    }

    [Test]
    public async Task CenterOfPark_ShouldReturnParkOptionsPosition()
    {
        await Assert.That(viewModel.CenterOfPark.Latitude).IsEqualTo(33.8121);
        await Assert.That(viewModel.CenterOfPark.Longitude).IsEqualTo(-117.9190);
    }

    [Test]
    public async Task MapStartZoomDistanceMeters_ShouldReturnParkOptionsValue()
    {
        await Assert.That(viewModel.MapStartZoomDistanceMeters).IsEqualTo(500);
    }

    [Test]
    public async Task OnAppearing_ShouldLoadParkingLocationFromAppSettings()
    {
        var expectedPosition = new Position(33.8121, -117.9190);
        appSettings.ParkingLocation = expectedPosition;

        viewModel.OnAppearing();

        await Assert.That(viewModel.ParkLocation).IsEqualTo(expectedPosition);
    }

    [Test]
    public async Task OnAppearing_WhenNoParkingLocation_ShouldHaveNullParkLocation()
    {
        appSettings.ParkingLocation = null;

        viewModel.OnAppearing();

        await Assert.That(viewModel.ParkLocation).IsNull();
    }

    [Test]
    public async Task Localize_ShouldReturnInjectedLocalize()
    {
        await Assert.That(viewModel.Localize).IsEqualTo(localize);
    }

    [Test]
    public void OnDisappearing_ShouldNotThrow()
    {
        viewModel.OnDisappearing();
    }

    [Test]
    public async Task IsBusy_ShouldStartAsFalse()
    {
        await Assert.That(viewModel.IsBusy).IsFalse();
    }
}
