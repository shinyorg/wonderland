namespace ShinyWonderland.Features.Hours.Pages;

[ShellMap<HoursPage>(
    description: "View the park hours for the upcoming week",
    registerRoute: false
)]
public partial class HoursViewModel(ViewModelServices services) : BaseViewModel(services)
{
    [ObservableProperty] List<VmParkSchedule> schedule;

    public async void OnAppearing()
    {
        try
        {
            var scheduleDates = await Mediator.Request(new GetUpcomingParkHours());

            var now = DateOnly.FromDateTime(Services.TimeProvider.GetLocalNow().Date);
            this.Schedule = scheduleDates
                .Result
                .Select(x =>
                {
                    var today = x.Date == now;
                    return new VmParkSchedule(x, today, this.Localize);
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load park hours");
        }
    }
}

public record VmParkSchedule(
    ParkHours Info,
    bool IsToday,
    StringsLocalized Localize
)
{
    public bool IsOpen => Info.IsOpen;
    public bool IsClosed => Info.IsClosed;
    public string HoursOfOperation => $"{Info.Hours?.Open:h:mm tt} - {Info.Hours?.Closed:h:mm tt}";
    public string DateString => IsToday ? Localize.Today : this.Info.Date.ToString("dddd, MMMM dd");
}
