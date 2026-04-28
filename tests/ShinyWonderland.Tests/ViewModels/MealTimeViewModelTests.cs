using ShinyWonderland.Features.MealTimes.Handlers;

namespace ShinyWonderland.Tests.ViewModels;

public class MealTimeViewModelTests
{
    readonly TestMediator mediator;
    readonly StringsLocalized localize;
    readonly CoreServices services;
    readonly MealTimeViewModel viewModel;

    public MealTimeViewModelTests()
    {
        mediator = new TestMediator();
        localize = TestLocalization.Create();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
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

        SetupDefaultAvailability();

        viewModel = new MealTimeViewModel(services);
    }

    void SetupDefaultAvailability()
    {
        var availability = new MealTimeAvailability(
            new List<MealPassAvailability>(),
            new List<MealPassAvailability>(),
            0, 0, null, null
        );
        mediator.SetupRequest<GetMealTimeAvailability, MealTimeAvailability>(availability);
    }

    [Test]
    public async Task OnAppearing_ShouldLoadHistory()
    {
        var history = new List<MealTimeHistoryRecord>
        {
            new() { Id = 1, Type = MealTimeType.Drink, Timestamp = DateTimeOffset.UtcNow },
            new() { Id = 2, Type = MealTimeType.Food, Timestamp = DateTimeOffset.UtcNow.AddHours(-1) }
        };

        mediator.SetupRequest<GetMealTimeHistory, List<MealTimeHistoryRecord>>(history);

        viewModel.OnAppearing();
        await Task.Delay(100);

        await Assert.That(viewModel.History).IsNotNull();
        await Assert.That(viewModel.History.Count).IsEqualTo(2);
    }

    [Test]
    public async Task OnAppearing_WithEmptyHistory_ShouldSetEmptyList()
    {
        var history = new List<MealTimeHistoryRecord>();

        mediator.SetupRequest<GetMealTimeHistory, List<MealTimeHistoryRecord>>(history);

        viewModel.OnAppearing();
        await Task.Delay(100);

        await Assert.That(viewModel.History).IsNotNull();
        await Assert.That(viewModel.History.Count).IsEqualTo(0);
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
    public async Task UseDrinkPass_ShouldSendCommand()
    {
        var history = new List<MealTimeHistoryRecord>();
        mediator.SetupRequest<GetMealTimeHistory, List<MealTimeHistoryRecord>>(history);

        await viewModel.UseDrinkPassCommand.ExecuteAsync(null);

        await Assert.That(mediator.SentCommands.OfType<UseMealPassCommand>())
            .Contains(x => x.Type == MealTimeType.Drink);
    }

    [Test]
    public async Task AddDrinkPass_ShouldSendCommand()
    {
        await viewModel.AddDrinkPassCommand.ExecuteAsync(null);

        await Assert.That(mediator.SentCommands.OfType<AddMealPassCommand>())
            .Contains(x => x.Type == MealTimeType.Drink);
    }

    [Test]
    public async Task OnAppearing_ShouldSetButtonTextForNoPasses()
    {
        var history = new List<MealTimeHistoryRecord>();
        mediator.SetupRequest<GetMealTimeHistory, List<MealTimeHistoryRecord>>(history);

        viewModel.OnAppearing();
        await Task.Delay(100);

        await Assert.That(viewModel.DrinkButtonText).IsEqualTo("Add Pass");
        await Assert.That(viewModel.FoodButtonText).IsEqualTo("Add Pass");
    }
}
