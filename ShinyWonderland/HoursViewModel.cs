using ShinyWonderland.ThemeParksApi;

namespace ShinyWonderland;

public partial class HoursViewModel(
    IMediator mediator, 
    IOptions<ParkOptions> parkOptions,
    TimeProvider timeProvider
) : ObservableObject, INavigatedAware
{
    [ObservableProperty] List<ParkSchedule> schedule;
    
    
    public async void OnNavigatedTo()
    {
        var scheduleDates = await mediator.Request(new GetEntityScheduleUpcomingHttpRequest
        {
            EntityID = parkOptions.Value.EntityId
        });
        
        var now = DateOnly.FromDateTime(timeProvider.GetLocalNow().Date);
        this.Schedule = scheduleDates
            .Result
            .Schedule
            .Where(x => x.Type == ScheduleEntryType.OPERATING)
            .Select(x =>
            {
                var date = DateOnly.FromDateTime(x.OpeningTime.LocalDateTime);
                var today = date == now;
                
                return new ParkSchedule(
                    DateOnly.FromDateTime(x.OpeningTime.LocalDateTime),
                    TimeOnly.FromDateTime(x.OpeningTime.LocalDateTime),
                    TimeOnly.FromDateTime(x.ClosingTime.LocalDateTime),
                    today
                );
            })
            .OrderBy(x => x.Date)
            .ToList();
    }

    
    public void OnNavigatedFrom()
    {
    }
}

public record ParkSchedule(
    DateOnly Date,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    bool IsToday
)
{
    public string DateString => IsToday ? "Today" : this.Date.ToString("dddd MMM dd");
}