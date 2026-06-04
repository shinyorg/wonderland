namespace ShinyWonderland.Features.Hours.Pages;

[ShellMap<HoursPage>(
    description: "View the park hours for the upcoming week",
    registerRoute: false
)]
public partial class HoursViewModel(ViewModelServices services) : BaseViewModel(services)
{
    [ObservableProperty] List<VmParkSchedule> schedule;
    [ObservableProperty] public partial bool IsLoading { get; private set; }

    public override void OnAppearing()
    {
        base.OnAppearing();
        if (this.Schedule == null || this.Schedule.Count == 0)
            this.IsLoading = true;

        _ = this.LoadData(false);
    }

    [RelayCommand]
    Task Load() => this.LoadData(true);

    async Task LoadData(bool forceRefresh)
    {
        if (forceRefresh)
            this.IsLoading = true;

        try
        {
            var scheduleDates = await Mediator.Request(
                new GetUpcomingParkHours(),
                this.DeactivateToken,
                ctx =>
                {
                    if (forceRefresh)
                        ctx.ForceCacheRefresh();
                }
            );

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
        finally
        {
            this.IsLoading = false;
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
