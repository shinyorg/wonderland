namespace ShinyWonderland.Tests.ViewModels;

public class MealTimeViewModelTests
{
    readonly IMediator mediator;
    readonly StringsLocalized localize;
    readonly IOptions<MealTimeOptions> options;
    readonly FakeTimeProvider timeProvider;
    readonly MealTimeViewModel viewModel;

    public MealTimeViewModelTests()
    {
        mediator = Substitute.For<IMediator>();
        localize = TestLocalization.Create();
        options = Options.Create(new MealTimeOptions
        {
            Enabled = true,
            DrinkTimeWait = TimeSpan.FromMinutes(15),
            FoodTimeWait = TimeSpan.FromMinutes(90)
        });
        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        SetupDefaultAvailability();

        viewModel = new MealTimeViewModel(mediator, options, timeProvider, localize);
    }

    void SetupDefaultAvailability()
    {
        var availability = new MealTimeAvailability(
            new List<MealPassAvailability>(),
            new List<MealPassAvailability>(),
            0, 0, null, null
        );
        mediator.Request(Arg.Any<GetMealTimeAvailability>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(availability));
    }

    [Fact]
    public async Task OnAppearing_ShouldLoadHistory()
    {
        // Arrange
        var history = new List<MealTimeHistoryRecord>
        {
            new() { Id = 1, Type = MealTimeType.Drink, Timestamp = DateTimeOffset.UtcNow },
            new() { Id = 2, Type = MealTimeType.Food, Timestamp = DateTimeOffset.UtcNow.AddHours(-1) }
        };

        mediator.Request(Arg.Any<GetMealTimeHistory>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(history));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100);

        // Assert
        viewModel.History.ShouldNotBeNull();
        viewModel.History.Count.ShouldBe(2);
    }

    [Fact]
    public async Task OnAppearing_WithEmptyHistory_ShouldSetEmptyList()
    {
        // Arrange
        var history = new List<MealTimeHistoryRecord>();

        mediator.Request(Arg.Any<GetMealTimeHistory>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(history));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100);

        // Assert
        viewModel.History.ShouldNotBeNull();
        viewModel.History.Count.ShouldBe(0);
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

    [Fact]
    public async Task UseDrinkPass_ShouldSendCommand()
    {
        // Arrange
        var history = new List<MealTimeHistoryRecord>();
        mediator.Request(Arg.Any<GetMealTimeHistory>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(history));

        // Act
        await viewModel.UseDrinkPassCommand.ExecuteAsync(null);

        // Assert
        await mediator.Received().Send(
            Arg.Is<UseMealPassCommand>(x => x.Type == MealTimeType.Drink),
            Arg.Any<CancellationToken>(),
            Arg.Any<Action<IMediatorContext>>()
        );
    }

    [Fact]
    public async Task AddDrinkPass_ShouldSendCommand()
    {
        // Act
        await viewModel.AddDrinkPassCommand.ExecuteAsync(null);

        // Assert
        await mediator.Received().Send(
            Arg.Is<AddMealPassCommand>(x => x.Type == MealTimeType.Drink),
            Arg.Any<CancellationToken>(),
            Arg.Any<Action<IMediatorContext>>()
        );
    }

    [Fact]
    public async Task OnAppearing_ShouldSetButtonTextForNoPasses()
    {
        // Arrange
        var history = new List<MealTimeHistoryRecord>();
        mediator.Request(Arg.Any<GetMealTimeHistory>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(history));

        // Act
        viewModel.OnAppearing();
        await Task.Delay(100);

        // Assert
        viewModel.DrinkButtonText.ShouldBe("Add Pass");
        viewModel.FoodButtonText.ShouldBe("Add Pass");
    }
}
