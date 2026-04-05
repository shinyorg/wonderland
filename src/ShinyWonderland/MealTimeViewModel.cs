using ShinyWonderland.Handlers;

namespace ShinyWonderland;


[ShellMap<MealTimePage>(registerRoute: false)]
public partial class MealTimeViewModel(
    IMediator mediator,
    IOptions<MealTimeOptions> options,
    TimeProvider timeProvider,
    StringsLocalized localize
) : ObservableObject, IPageLifecycleAware
{
    IDisposable? timerSub;

    [ObservableProperty] List<MealTimeHistoryRecord> history;
    [ObservableProperty] MealTimeAvailability? availability;
    [ObservableProperty] string drinkButtonText = "";
    [ObservableProperty] string foodButtonText = "";
    [ObservableProperty] bool isDrinkAvailable;
    [ObservableProperty] bool isFoodAvailable;
    [ObservableProperty] int drinkPassCount;
    [ObservableProperty] int foodPassCount;

    public StringsLocalized Localize => localize;

    public async void OnAppearing()
    {
        this.History = (await mediator.Request(new GetMealTimeHistory())).Result;
        await RefreshAvailability();

        this.timerSub = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Subscribe(async _ => await RefreshAvailability());
    }

    public void OnDisappearing()
    {
        this.timerSub?.Dispose();
        this.timerSub = null;
    }

    async Task RefreshAvailability()
    {
        var result = (await mediator.Request(new GetMealTimeAvailability())).Result;
        this.Availability = result;
        this.DrinkPassCount = result.DrinkPassCount;
        this.FoodPassCount = result.FoodPassCount;
        this.IsDrinkAvailable = result.HasDrinkPasses && result.IsDrinkAvailable;
        this.IsFoodAvailable = result.HasFoodPasses && result.IsFoodAvailable;
        this.DrinkButtonText = GetButtonText(result.DrinkPassCount, result.IsDrinkAvailable, result.NextDrinkAvailableIn);
        this.FoodButtonText = GetButtonText(result.FoodPassCount, result.IsFoodAvailable, result.NextFoodAvailableIn);
    }

    static string GetButtonText(int passCount, bool isAvailable, TimeSpan? nextAvailableIn)
    {
        if (passCount == 0)
            return "Add Pass";
        if (isAvailable)
            return "Use Pass";
        if (nextAvailableIn != null)
            return $"{(int)nextAvailableIn.Value.TotalMinutes:D2}:{nextAvailableIn.Value.Seconds:D2}";
        return "Use Pass";
    }

    [RelayCommand]
    async Task UseDrinkPass()
    {
        await mediator.Send(new UseMealPassCommand(MealTimeType.Drink));
        this.History = (await mediator.Request(new GetMealTimeHistory())).Result;
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task UseFoodPass()
    {
        await mediator.Send(new UseMealPassCommand(MealTimeType.Food));
        this.History = (await mediator.Request(new GetMealTimeHistory())).Result;
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task AddDrinkPass()
    {
        await mediator.Send(new AddMealPassCommand(MealTimeType.Drink));
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task AddFoodPass()
    {
        await mediator.Send(new AddMealPassCommand(MealTimeType.Food));
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task RemoveDrinkPass()
    {
        if (this.Availability?.DrinkPasses.Count > 0)
        {
            var pass = this.Availability.DrinkPasses.Last();
            await mediator.Send(new RemoveMealPassCommand(pass.PassId));
            await RefreshAvailability();
        }
    }

    [RelayCommand]
    async Task RemoveFoodPass()
    {
        if (this.Availability?.FoodPasses.Count > 0)
        {
            var pass = this.Availability.FoodPasses.Last();
            await mediator.Send(new RemoveMealPassCommand(pass.PassId));
            await RefreshAvailability();
        }
    }
}
