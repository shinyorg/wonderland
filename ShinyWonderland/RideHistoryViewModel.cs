using ShinyWonderland.Handlers;

namespace ShinyWonderland;


[ShellMap<RideHistoryPage>]
public partial class RideHistoryViewModel(
    IMediator mediator,
    Humanizer humanizer,
    RideHistoryViewModelLocalized localize
) : ObservableObject, IPageLifecycleAware
{
    public RideHistoryViewModelLocalized Localize => localize;
    [ObservableProperty] List<RideHistoryItemViewModel> history;
    public Guid? RideId { get; set; }
    
    public async void OnAppearing()
    {
        var items = await mediator.Request(new GetRideHistory(this.RideId));
        this.History = items.Result.Select(x => new RideHistoryItemViewModel(humanizer, x)).ToList();
    }

    
    public void OnDisappearing()
    {
    }
}

public class RideHistoryItemViewModel(Humanizer humanizer, RideHistoryRecord record)
{
    public string TimeAgo => humanizer.TimeAgo(record.Timestamp);
    public string RideName => record.RideName;
    public RideHistoryRecord Data => record;
}