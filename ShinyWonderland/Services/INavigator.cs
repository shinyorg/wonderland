namespace ShinyWonderland.Services;

public interface INavigator
{
    Task NavigateTo(string route, params IEnumerable<(string Key, object Value)> args);
    Task Alert(string title, string message);
    Task<bool> Confirm(string title, string message);
}

public interface INavigatedAware
{
    void OnNavigatedTo();
    void OnNavigatedFrom();
}