using ShinyWonderland.Handlers;

namespace ShinyWonderland;


[ShellMap<MealTimePage>(registerRoute: false)]
public partial class MealTimeViewModel(
    IMediator mediator,
    INavigator navigator,
    IOptions<MealTimeOptions> options,
    MealTimeViewModelLocalized localize
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<MealTimeHistoryRecord> history;
    public MealTimeViewModelLocalized Localize => localize;
    
    public async void OnAppearing()
    {
        this.History = (await mediator.Request(new GetMealTimeHistory())).Result;
        // TODO: set timers
    }
    

    public void OnDisappearing()
    {
    }
}