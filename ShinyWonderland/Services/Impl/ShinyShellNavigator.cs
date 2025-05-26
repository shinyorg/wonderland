namespace ShinyWonderland.Services.Impl;


// https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0
public class ShinyShellNavigator(
    ILogger<ShinyShellNavigator> logger,
    IApplication application,
    ShinyNavigationBuilder navBuilder
) : INavigator, IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        if (application is not Application app)
            throw new InvalidOperationException($"Invalid MAUI Application - {application.GetType()}");

        app.DescendantRemoved += (_, args) =>
        {
            if (args.Element is Page { BindingContext: IDisposable disposable })
            {
                logger.LogDebug("[Dispose] ViewModel '{type}'", disposable.GetType());
                disposable.Dispose();
            }
        };
        
        app.PageAppearing += (_, page) =>
        {
            if (page.BindingContext == null)
            {
                // needed for initial pags - IQueryAttributable would be missed
                var viewModelType = navBuilder.GetViewModelTypeForPage(page.GetType());
                if (viewModelType == null)
                {
                    logger.LogDebug("No ViewModel found for page");
                }
                else
                {
                    var vm = services.GetService(viewModelType);
                    page.BindingContext = vm;
                    logger.LogDebug("[Binding] ViewModel {type} set on page", viewModelType);
                }
            }
            
            if (page.BindingContext is IPageLifecycleAware lc)
            {
                logger.LogDebug("[OnAppearing] ViewModel '{type}' ", lc.GetType());
                lc.OnAppearing();
            }
        };

        app.PageDisappearing += (_, page) =>
        {
            if (page.BindingContext is IPageLifecycleAware lc)
            {
                logger.LogDebug("[OnAppearing] ViewModel '{type}' ", lc.GetType());
                lc.OnDisappearing();
            }
        };
    }
    
    
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