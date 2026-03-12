namespace ShinyWonderland.Tests.ViewModels;

public class RideHistoryViewModelTests
{
    readonly IMediator mediator;
    readonly FakeTimeProvider timeProvider;
    readonly Humanizer humanizer;
    readonly StringsLocalized localize;
    readonly RideHistoryViewModel viewModel;

    public RideHistoryViewModelTests()
    {
        mediator = Substitute.For<IMediator>();
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

        viewModel = new RideHistoryViewModel(mediator, humanizer, localize);
    }

    [Fact]
    public async Task OnAppearing_ShouldLoadHistory()
    {
        // Arrange
        var records = new List<RideHistoryRecord>
        {
            new() { Id = 1, RideId = "ride1", RideName = "Thunder Mountain", Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30) },
            new() { Id = 2, RideId = "ride2", RideName = "Space Mountain", Timestamp = DateTimeOffset.UtcNow.AddHours(-1) }
        };

        mediator.Request(Arg.Any<GetRideHistory>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(records));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100);

        // Assert
        viewModel.History.ShouldNotBeNull();
        viewModel.History.Count.ShouldBe(2);
    }

    [Fact]
    public async Task OnAppearing_WithRideIdFilter_ShouldPassRideIdToRequest()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        viewModel.RideId = rideId;

        var records = new List<RideHistoryRecord>();
        GetRideHistory? capturedRequest = null;

        mediator.Request(Arg.Do<GetRideHistory>(r => capturedRequest = r), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(records));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Ride.ShouldBe(rideId);
    }

    [Fact]
    public void RideHistoryItemViewModel_ShouldReturnCorrectRideName()
    {
        // Arrange
        var record = new RideHistoryRecord
        {
            Id = 1,
            RideId = "ride1",
            RideName = "Thunder Mountain",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        var itemVm = new RideHistoryItemViewModel(humanizer, record);

        // Assert
        itemVm.RideName.ShouldBe("Thunder Mountain");
        itemVm.Data.ShouldBe(record);
    }

    [Fact]
    public void RideHistoryItemViewModel_TimeAgo_ShouldReturnFormattedTime()
    {
        // Arrange - use FakeTimeProvider time for deterministic results
        var record = new RideHistoryRecord
        {
            Id = 1,
            RideId = "ride1",
            RideName = "Thunder Mountain",
            Timestamp = timeProvider.GetUtcNow().AddMinutes(-5)
        };

        var itemVm = new RideHistoryItemViewModel(humanizer, record);

        // Assert
        itemVm.TimeAgo.ShouldNotBeNullOrEmpty();
        itemVm.TimeAgo.ShouldContain("5");
        itemVm.TimeAgo.ShouldContain("minute");
    }

    [Fact]
    public void Localize_ShouldReturnInjectedLocalize()
    {
        // Assert
        viewModel.Localize.ShouldBe(localize);
    }

    [Fact]
    public void OnDisappearing_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => viewModel.OnDisappearing());
    }
}