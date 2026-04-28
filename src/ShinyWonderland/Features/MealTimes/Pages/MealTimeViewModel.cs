namespace ShinyWonderland.Features.MealTimes.Pages;


[ShellMap<MealTimePage>(registerRoute: false)]
public partial class MealTimeViewModel(CoreServices services) : BaseViewModel(services)
{
    [ObservableProperty] List<MealTimeHistoryRecord> history;
    [ObservableProperty] MealTimeAvailability? availability;
    [ObservableProperty] string drinkButtonText = "";
    [ObservableProperty] string foodButtonText = "";
    [ObservableProperty] bool isDrinkAvailable;
    [ObservableProperty] bool isFoodAvailable;
    [ObservableProperty] int drinkPassCount;
    [ObservableProperty] int foodPassCount;

    public override async void OnAppearing()
    {
        this.History = (await Mediator.Request(new GetMealTimeHistory())).Result;
        await RefreshAvailability();

        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Subscribe(async _ => await RefreshAvailability())
            .DisposedBy(this.DeactivateWith);
    }


    async Task RefreshAvailability()
    {
        var result = (await Mediator.Request(new GetMealTimeAvailability())).Result;
        this.Availability = result;
        this.DrinkPassCount = result.DrinkPassCount;
        this.FoodPassCount = result.FoodPassCount;
        this.IsDrinkAvailable = result is { HasDrinkPasses: true, IsDrinkAvailable: true };
        this.IsFoodAvailable = result is { HasFoodPasses: true, IsFoodAvailable: true };
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
        await Mediator.Send(new UseMealPassCommand(MealTimeType.Drink));
        this.History = (await Mediator.Request(new GetMealTimeHistory())).Result;
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task UseFoodPass()
    {
        await Mediator.Send(new UseMealPassCommand(MealTimeType.Food));
        this.History = (await Mediator.Request(new GetMealTimeHistory())).Result;
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task AddDrinkPass()
    {
        await Mediator.Send(new AddMealPassCommand(MealTimeType.Drink));
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task AddFoodPass()
    {
        await Mediator.Send(new AddMealPassCommand(MealTimeType.Food));
        await RefreshAvailability();
    }

    [RelayCommand]
    async Task RemoveDrinkPass()
    {
        if (this.Availability?.DrinkPasses.Count > 0)
        {
            var pass = this.Availability.DrinkPasses.Last();
            await Mediator.Send(new RemoveMealPassCommand(pass.PassId));
            await RefreshAvailability();
        }
    }

    [RelayCommand]
    async Task RemoveFoodPass()
    {
        if (this.Availability?.FoodPasses.Count > 0)
        {
            var pass = this.Availability.FoodPasses.Last();
            await Mediator.Send(new RemoveMealPassCommand(pass.PassId));
            await RefreshAvailability();
        }
    }
}
