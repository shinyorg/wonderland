namespace ShinyWonderland.Services.Impl;

public class ShinyShellNavigator : INavigator
{
    public async Task NavigateTo(string uri, params IEnumerable<(string Key, object Value)> args)
    {
        var parameters = args.ToDictionary(x => x.Key, x => x.Value);
        await Shell.Current.GoToAsync(uri, true, parameters);
    }

    
    public Task GoBack() => MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync(".."));


    public async Task Alert(string title, string message)
    {
        var tcs = new TaskCompletionSource();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlert(title, message, "OK");
            tcs.SetResult();
        });
        await tcs.Task;
    }
    

    public async Task<bool> Confirm(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var result = await Shell.Current.DisplayAlert(title, message, "Yes", "No");
            tcs.SetResult(result);
        });
        return await tcs.Task.ConfigureAwait(false);
    }
}