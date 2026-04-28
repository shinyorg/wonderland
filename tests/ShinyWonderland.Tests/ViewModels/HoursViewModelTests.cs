using ShinyWonderland.Contracts;

namespace ShinyWonderland.Tests.ViewModels;

public class HoursViewModelTests
{
    readonly TestMediator mediator;
    readonly FakeTimeProvider timeProvider;
    readonly StringsLocalized localize;
    readonly CoreServices services;
    readonly HoursViewModel viewModel;

    public HoursViewModelTests()
    {
        mediator = new TestMediator();
        timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 1, 26, 10, 0, 0, TimeSpan.Zero));
        localize = TestLocalization.Create(new Dictionary<string, string>
        {
            ["Today"] = "Today"
        });

        var parkOptions = Options.Create(new ParkOptions
        {
            Name = "Wonderland",
            EntityId = "test-park",
            Latitude = 33.8121,
            Longitude = -117.9190,
            NotificationDistanceMeters = 1000
        });

        services = new CoreServices(
            mediator,
            parkOptions,
            new AppSettings(),
            new TestNavigator(),
            new IDialogsImposter().Instance(),
            timeProvider,
            new IGpsManagerImposter().Instance(),
            localize,
            new INotificationManagerImposter().Instance(),
            NullLoggerFactory.Instance
        );

        viewModel = new HoursViewModel(services);
    }

    [Test]
    public async Task OnAppearing_ShouldLoadSchedule()
    {
        var now = timeProvider.GetLocalNow();

        var parkHours = new ParkHours[]
        {
            new(DateOnly.FromDateTime(now.Date), new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0))),
            new(DateOnly.FromDateTime(now.Date.AddDays(1)), new TimeRange(new TimeOnly(9, 0), new TimeOnly(23, 0))),
            new(DateOnly.FromDateTime(now.Date.AddDays(2)), null) // Closed day
        };

        mediator.SetupRequest<GetUpcomingParkHours, ParkHours[]>(parkHours);

        viewModel.OnAppearing();
        await Task.Delay(100);

        await Assert.That(viewModel.Schedule).IsNotNull();
        await Assert.That(viewModel.Schedule.Count).IsEqualTo(3);
        await Assert.That(viewModel.Schedule[0].IsToday).IsTrue();
        await Assert.That(viewModel.Schedule[1].IsToday).IsFalse();
        await Assert.That(viewModel.Schedule[2].IsClosed).IsTrue();
    }

    [Test]
    public async Task VmParkSchedule_ShouldReturnCorrectTodayString_WhenIsToday()
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkHours = new ParkHours(date, new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0)));
        var schedule = new VmParkSchedule(parkHours, true, localize);

        await Assert.That(schedule.DateString).IsEqualTo("Today");
        await Assert.That(schedule.IsOpen).IsTrue();
    }

    [Test]
    public async Task VmParkSchedule_ShouldReturnFormattedDate_WhenNotToday()
    {
        var date = new DateOnly(2026, 1, 27);
        var parkHours = new ParkHours(date, new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0)));
        var schedule = new VmParkSchedule(parkHours, false, localize);

        await Assert.That(schedule.DateString).IsNotEqualTo("Today");
        await Assert.That(schedule.DateString).Contains("January");
    }

    [Test]
    public async Task VmParkSchedule_HoursOfOperation_ShouldFormatCorrectly()
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkHours = new ParkHours(date, new TimeRange(new TimeOnly(9, 0), new TimeOnly(22, 0)));
        var schedule = new VmParkSchedule(parkHours, true, localize);

        await Assert.That(schedule.HoursOfOperation).Contains("9:00");
        await Assert.That(schedule.HoursOfOperation).Contains("10:00");
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
