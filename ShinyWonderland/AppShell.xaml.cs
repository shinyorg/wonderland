namespace ShinyWonderland;


public partial class AppShell : Shell
{
    public AppShell(AppShellLocalized localize)
    {
        this.InitializeComponent();
        
        this.AddTab<RideTimesPage>(localize.RideTimes, "ride_time");
        this.AddTab<MapRideTimesPage>(localize.RideTimes, "ride_time");
        this.AddTab<SettingsPage>(localize.Settings, "settings");
        this.AddTab<ParkingPage>(localize.Parking, "parking");
        this.AddTab<HoursPage>(localize.Hours, "hours");
#if DEBUG
        this.AddTab<MealTimePage>(localize.MealTimes, "meal");
#endif
    }


    void AddTab<TPage>(string title, string icon) where TPage : Page, new()
    {
        this.Items.Add(new Tab
        {
            Title = title,
            Icon = icon,
            Items = {
                new ShellContent
                {
                    ContentTemplate = new DataTemplate(() => new TPage())
                }
            }
        });
    }
}