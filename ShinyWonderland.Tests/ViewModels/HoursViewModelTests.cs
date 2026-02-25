using System.ComponentModel;

namespace ShinyWonderland.Tests.ViewModels;

public class HoursViewModelTests
{
    readonly IMediator mediator;
    readonly FakeTimeProvider timeProvider;
    readonly HoursViewModelLocalized localize;
    readonly HoursViewModel viewModel;

    public HoursViewModelTests()
    {
        mediator = Substitute.For<IMediator>();
        timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 1, 26, 10, 0, 0, TimeSpan.Zero));
        localize = Substitute.For<HoursViewModelLocalized>();
        localize.Today.Returns("Today");

        viewModel = new HoursViewModel(mediator, timeProvider, localize);
    }

    [Fact]
    public async Task OnAppearing_ShouldLoadSchedule()
    {
        // Arrange
        var now = timeProvider.GetLocalNow();

        var parkHours = new ParkHours[]
        {
            new(DateOnly.FromDateTime(now.Date), new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0))),
            new(DateOnly.FromDateTime(now.Date.AddDays(1)), new TimeRange(new TimeOnly(9, 0), new TimeOnly(23, 0))),
            new(DateOnly.FromDateTime(now.Date.AddDays(2)), null) // Closed day
        };

        mediator.Request(Arg.Any<GetUpcomingParkHours>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(parkHours));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        viewModel.Schedule.ShouldNotBeNull();
        viewModel.Schedule.Count.ShouldBe(3);
        viewModel.Schedule[0].IsToday.ShouldBeTrue();
        viewModel.Schedule[1].IsToday.ShouldBeFalse();
        viewModel.Schedule[2].IsClosed.ShouldBeTrue();
    }

    [Fact]
    public void VmParkSchedule_ShouldReturnCorrectTodayString_WhenIsToday()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkHours = new ParkHours(date, new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0)));
        var schedule = new VmParkSchedule(parkHours, true, localize);

        // Assert
        schedule.DateString.ShouldBe("Today");
        schedule.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public void VmParkSchedule_ShouldReturnFormattedDate_WhenNotToday()
    {
        // Arrange
        var date = new DateOnly(2026, 1, 27);
        var parkHours = new ParkHours(date, new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0)));
        var schedule = new VmParkSchedule(parkHours, false, localize);

        // Assert
        schedule.DateString.ShouldNotBe("Today");
        schedule.DateString.ShouldContain("January");
    }

    [Fact]
    public void VmParkSchedule_HoursOfOperation_ShouldFormatCorrectly()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkHours = new ParkHours(date, new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0)));
        var schedule = new VmParkSchedule(parkHours, true, localize);

        // Assert
        schedule.HoursOfOperation.ShouldContain("9:00");
        schedule.HoursOfOperation.ShouldContain("10:00");
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