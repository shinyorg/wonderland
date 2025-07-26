namespace ShinyWonderland;


public partial class AppShell : Shell
{
    public AppShell()
    {
        this.InitializeComponent();
#if DEBUG
        var mealTimeTab = new Tab
        {
            Title = "Meal Times",
            Icon = "meal",
            Items = {
                new ShellContent
                {
                    ContentTemplate = new DataTemplate(() => new MealTimePage())
                }
            }
        };
        this.Items.Add(mealTimeTab);
#endif
    }
}