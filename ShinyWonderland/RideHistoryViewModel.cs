using ShinyWonderland.Handlers;

namespace ShinyWonderland;


[ShellMap<RideHistoryPage>]
public partial class RideHistoryViewModel(
    IMediator mediator
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<RideHistoryRecord> history;
    
    public async void OnAppearing()
    {
        this.History = (await mediator.Request(new GetRideHistory())).Result;
    }

    
    public void OnDisappearing()
    {
    }
}