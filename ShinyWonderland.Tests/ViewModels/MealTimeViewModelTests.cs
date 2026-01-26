namespace ShinyWonderland.Tests.ViewModels;

public class MealTimeViewModelTests
{
    readonly IMediator mediator;
    readonly MealTimeViewModelLocalized localize;
    readonly MealTimeViewModel viewModel;

    public MealTimeViewModelTests()
    {
        mediator = Substitute.For<IMediator>();
        localize = Substitute.For<MealTimeViewModelLocalized>();

        viewModel = new MealTimeViewModel(mediator, localize);
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
        await Task.Delay(100); // Allow async operation to complete

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
}