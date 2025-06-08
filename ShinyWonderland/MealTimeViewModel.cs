using ShinyWonderland.Handlers;

namespace ShinyWonderland;


[ShellMap<MealTimePage>]
public partial class MealTimeViewModel(
    IMediator mediator,
    INavigator navigator,
    IOptions<MealTimeOptions> options
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<MealTimeHistoryRecord> history;
    
    public async void OnAppearing()
    {
        this.History = (await mediator.Request(new GetMealTimeHistory())).Result;
        // TODO: set timers
    }
    

    public void OnDisappearing()
    {
    }
}