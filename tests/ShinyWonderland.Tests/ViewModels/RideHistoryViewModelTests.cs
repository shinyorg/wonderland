namespace ShinyWonderland.Tests.ViewModels;

public class RideHistoryViewModelTests
{
    readonly TestMediator mediator;
    readonly FakeTimeProvider timeProvider;
    readonly Humanizer humanizer;
    readonly StringsLocalized localize;
    readonly CoreServices services;
    readonly RideHistoryViewModel viewModel;

    public RideHistoryViewModelTests()
    {
        mediator = new TestMediator();
        localize = TestLocalization.Create(new Dictionary<string, string>
        {
            ["Never"] = "Never",
            ["Second"] = "second",
            ["Seconds"] = "seconds",
            ["Minute"] = "minute",
            ["Minutes"] = "minutes",
            ["Hour"] = "hour",
            ["Hours"] = "hours",
            ["Day"] = "day",
            ["Days"] = "days",
            ["Month"] = "month",
            ["Months"] = "months",
            ["Year"] = "year",
            ["Years"] = "years"
        });

        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        humanizer = new Humanizer(timeProvider, localize);

        services = new CoreServices(
            mediator,
            Options.Create(new ParkOptions
            {
                Name = "Wonderland",
                EntityId = "test-park",
                Latitude = 33.8121,
                Longitude = -117.9190,
                NotificationDistanceMeters = 1000
            }),
            new AppSettings(),
            new TestNavigator(),
            new IDialogsImposter().Instance(),
            timeProvider,
            new IGpsManagerImposter().Instance(),
            localize,
            new INotificationManagerImposter().Instance(),
            NullLoggerFactory.Instance
        );

        viewModel = new RideHistoryViewModel(services, humanizer);
    }

    [Test]
    public async Task OnAppearing_ShouldLoadHistory()
    {
        var records = new List<RideHistoryRecord>
        {
            new() { Id = 1, RideId = "ride1", RideName = "Thunder Mountain", Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30) },
            new() { Id = 2, RideId = "ride2", RideName = "Space Mountain", Timestamp = DateTimeOffset.UtcNow.AddHours(-1) }
        };

        mediator.SetupRequest<GetRideHistory, List<RideHistoryRecord>>(records);

        viewModel.OnAppearing();
        await Task.Delay(100);

        await Assert.That(viewModel.History).IsNotNull();
        await Assert.That(viewModel.History.Count).IsEqualTo(2);
    }

    [Test]
    public async Task OnAppearing_WithRideIdFilter_ShouldPassRideIdToRequest()
    {
        var rideId = Guid.NewGuid();
        viewModel.RideId = rideId;

        var records = new List<RideHistoryRecord>();
        mediator.SetupRequest<GetRideHistory, List<RideHistoryRecord>>(records);

        viewModel.OnAppearing();
        await Task.Delay(100);

        var capturedRequest = mediator.Requests.OfType<GetRideHistory>().LastOrDefault();
        await Assert.That(capturedRequest).IsNotNull();
        await Assert.That(capturedRequest!.Ride).IsEqualTo(rideId);
    }

    [Test]
    public async Task RideHistoryItemViewModel_ShouldReturnCorrectRideName()
    {
        var record = new RideHistoryRecord
        {
            Id = 1,
            RideId = "ride1",
            RideName = "Thunder Mountain",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        var itemVm = new RideHistoryItemViewModel(humanizer, record);

        await Assert.That(itemVm.RideName).IsEqualTo("Thunder Mountain");
        await Assert.That(itemVm.Data).IsEqualTo(record);
    }

    [Test]
    public async Task RideHistoryItemViewModel_TimeAgo_ShouldReturnFormattedTime()
    {
        var record = new RideHistoryRecord
        {
            Id = 1,
            RideId = "ride1",
            RideName = "Thunder Mountain",
            Timestamp = timeProvider.GetUtcNow().AddMinutes(-5)
        };

        var itemVm = new RideHistoryItemViewModel(humanizer, record);

        await Assert.That(itemVm.TimeAgo).IsNotNull();
        await Assert.That(itemVm.TimeAgo).IsNotEmpty();
        await Assert.That(itemVm.TimeAgo).Contains("5");
        await Assert.That(itemVm.TimeAgo).Contains("minute");
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
}
