using ShinyWonderland.Contracts;
using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;

public partial class HoursViewModel(
    IMediator mediator, 
    IOptions<ParkOptions> parkOptions,
    TimeProvider timeProvider
) : ObservableObject, INavigatedAware
{
    [ObservableProperty] List<VmParkSchedule> schedule;
    
    
    public async void OnNavigatedTo()
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

    
    public void OnNavigatedFrom()
    {
    }
}

public record VmParkSchedule(
    ParkHours Info,
    bool IsToday
)
{
    public string DateString => IsToday ? "Today" : this.Info.Date.ToString("dddd MMM dd");
}