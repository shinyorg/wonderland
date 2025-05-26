namespace ShinyWonderland.Services;

public interface INavigator
{
    // put IQueryAttributable on page/viewmodel to receive args
    Task NavigateTo(string route, params IEnumerable<(string Key, object Value)> args);
    Task GoBack();
    Task Alert(string title, string message);
    Task<bool> Confirm(string title, string message);
}


public interface INavigateConfirm
{
    Task<bool> CanNavigate();
}


public interface IPageLifecycleAware
{
    void OnAppearing();
    void OnDisappearing();
}