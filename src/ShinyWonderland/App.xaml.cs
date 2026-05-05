namespace ShinyWonderland;

public partial class App : Application
{
    readonly StringsLocalized localize;
    readonly IOptions<Features.MealTimes.MealTimeOptions> mealTimeOptions;

    public App(StringsLocalized localize, IOptions<Features.MealTimes.MealTimeOptions> mealTimeOptions)
    {
        this.localize = localize;
        this.mealTimeOptions = mealTimeOptions;
        this.InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new AppShell(this.localize, this.mealTimeOptions));
}
