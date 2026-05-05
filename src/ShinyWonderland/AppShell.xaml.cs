namespace ShinyWonderland;


public partial class AppShell : ShinyShell
{
    public AppShell(StringsLocalized localize, IOptions<Features.MealTimes.MealTimeOptions> mealTimeOptions)
    {
        this.Localize = localize;
        this.IsMealPassEnabled = mealTimeOptions.Value.Enabled;
        this.BindingContext = this;
        this.InitializeComponent();
    }

    public StringsLocalized Localize { get; }
    public bool IsMealPassEnabled { get; }
}
