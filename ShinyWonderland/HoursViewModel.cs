using ShinyWonderland.Contracts;

namespace ShinyWonderland;


[ShellMap<HoursPage>(registerRoute: false)]
public partial class HoursViewModel(
    IMediator mediator, 
    TimeProvider timeProvider,
    HoursViewModelLocalized localize
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<VmParkSchedule> schedule;
    public HoursViewModelLocalized Localize => localize;

    public async void OnAppearing()
    {
        var scheduleDates = await mediator.Request(new GetUpcomingParkHours());
        
        var now = DateOnly.FromDateTime(timeProvider.GetLocalNow().Date);
        this.Schedule = scheduleDates
            .Result
            .Select(x =>
            {
                var today = x.Date == now;
                return new VmParkSchedule(x, today, localize);
            })
            .ToList();
    }


    public void OnDisappearing()
    {
    }
}

public record VmParkSchedule(
    ParkHours Info,
    bool IsToday,
    HoursViewModelLocalized Localize
)
{
    public bool IsOpen => Info.IsOpen;
    public bool IsClosed => Info.IsClosed;
    public string HoursOfOperation => $"{Info.Hours?.Open:h:mm tt} - {Info.Hours?.Closed:h:mm tt}";
    public string DateString => IsToday ? Localize.Today : this.Info.Date.ToString("dddd, MMMM dd");
}