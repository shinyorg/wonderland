namespace ShinyWonderland;


[ShellMap<MealTimePage>]
public partial class RideHistoryViewModel(
    IMediator mediator
) : ObservableObject, IPageLifecycleAware
{
    public void OnAppearing()
    {
    }

    
    public void OnDisappearing()
    {
    }
}