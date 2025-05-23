namespace ShinyWonderland.Services.Impl;

public class ShellNavigator : INavigator
{
    public Task NavigateTo(string route, params IEnumerable<(string Key, object Value)> args)
    {
        var dict = args.ToDictionary(x => x.Key, x => x.Value);
        return Shell.Current.GoToAsync(route, true, dict);
    }
}