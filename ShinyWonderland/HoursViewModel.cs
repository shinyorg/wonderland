using ShinyWonderland.Contracts;

namespace ShinyWonderland;


public partial class HoursViewModel(
    IMediator mediator, 
    TimeProvider timeProvider
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<VmParkSchedule> schedule;


    public async void OnAppearing()
    {
        var scheduleDates = await mediator.Request(new GetUpcomingParkHours());
        
        var now = DateOnly.FromDateTime(timeProvider.GetLocalNow().Date);
        this.Schedule = scheduleDates
            .Result
            .Select(x =>
            {
                var today = x.Date == now;
                return new VmParkSchedule(x, today);
            })
            .ToList();
    }


    public void OnDisappearing()
    {
    }
}

public record VmParkSchedule(
    ParkHours Info,
    bool IsToday
)
{
    public bool IsOpen => Info.IsOpen;
    public string HoursOfOperation => $"{Info.Hours?.Open:h:mm tt} - {Info.Hours?.Closed:h:mm tt}";
    public string DateString => IsToday ? "Today" : this.Info.Date.ToString("dddd, MMMM dd");
}