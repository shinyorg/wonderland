using ShinyWonderland.Handlers;

namespace ShinyWonderland;


[ShellMap<RideHistoryPage>]
public partial class RideHistoryViewModel(
    IMediator mediator
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<RideHistoryRecord> history;
    public Guid? RideId { get; set; }
    
    public async void OnAppearing()
    {
        this.History = (await mediator.Request(new GetRideHistory(this.RideId))).Result;
    }

    
    public void OnDisappearing()
    {
    }
}