using ShinyWonderland.Features.Rides.Handlers;

namespace ShinyWonderland.Features.Rides.Pages;


[ShellMap<RideHistoryPage>]
public partial class RideHistoryViewModel(
    ViewModelServices services,
    Humanizer humanizer
) : BaseViewModel(services)
{
    [ObservableProperty] List<RideHistoryItemViewModel> history;
    public Guid? RideId { get; set; }
    
    public override async void OnAppearing()
    {
        try
        {
            var items = await Mediator.Request(new GetRideHistory(this.RideId));
            this.History = items.Result.Select(x => new RideHistoryItemViewModel(humanizer, x)).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load ride history");
        }
    }
}

public class RideHistoryItemViewModel(Humanizer humanizer, RideHistoryRecord record)
{
    public string TimeAgo => humanizer.TimeAgo(record.Timestamp);
    public string RideName => record.RideName;
    public RideHistoryRecord Data => record;
}
