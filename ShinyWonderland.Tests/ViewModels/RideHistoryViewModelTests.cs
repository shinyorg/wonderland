namespace ShinyWonderland.Tests.ViewModels;

public class RideHistoryViewModelTests
{
    readonly IMediator mediator;
    readonly FakeTimeProvider timeProvider;
    readonly Humanizer humanizer;
    readonly RideHistoryViewModelLocalized localize;
    readonly RideHistoryViewModel viewModel;

    public RideHistoryViewModelTests()
    {
        mediator = Substitute.For<IMediator>();
        localize = Substitute.For<RideHistoryViewModelLocalized>();

        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var humanizerLocalized = Substitute.For<HumanizerLocalized>();
        humanizerLocalized.Never.Returns("Never");
        humanizerLocalized.Second.Returns("second");
        humanizerLocalized.Seconds.Returns("seconds");
        humanizerLocalized.Minute.Returns("minute");
        humanizerLocalized.Minutes.Returns("minutes");
        humanizerLocalized.Hour.Returns("hour");
        humanizerLocalized.Hours.Returns("hours");
        humanizerLocalized.Day.Returns("day");
        humanizerLocalized.Days.Returns("days");
        humanizerLocalized.Month.Returns("month");
        humanizerLocalized.Months.Returns("months");
        humanizerLocalized.Year.Returns("year");
        humanizerLocalized.Years.Returns("years");

        humanizer = new Humanizer(timeProvider, humanizerLocalized);

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