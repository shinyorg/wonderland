namespace ShinyWonderland.Services;

public interface INavigator
{
    Task NavigateTo(string route, params IEnumerable<(string Key, object Value)> args);
}

public interface INavigatedAware
{
    void OnNavigatedTo();
}