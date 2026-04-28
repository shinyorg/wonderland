using System.Reactive.Disposables;

namespace ShinyWonderland.Features;


public partial class BaseViewModel(CoreServices services) : 
    ObservableObject, 
    IPageLifecycleAware, 
    INavigationConfirmation,
    IConnectivityEventHandler,
    IDisposable
{
    protected CoreServices Services => services;
    protected INavigator Navigator => services.Navigator;
    protected IDialogs Dialogs => services.Dialogs;
    protected IMediator Mediator => services.Mediator;
    public StringsLocalized Localize => services.Localized;
    
    [ObservableProperty]
    public partial bool IsBusy { get; protected set; }
    
    [ObservableProperty]
    public virtual partial string? Title { get; protected set; }
    
    [ObservableProperty]
    public partial bool IsInternetAvailable { get; private set; }
    
    public virtual void OnAppearing()
    {
        this.IsVisible = true;
    }

    public virtual void OnDisappearing()
    {
        this.Deactivate();
        this.IsVisible = false;
    }
    
    public virtual Task<bool> CanNavigate() => Task.FromResult(true);


    CompositeDisposable? deactivateWith;
    protected CompositeDisposable DeactivateWith
    {
        get
        {
            this.deactivateWith ??= new();
            return this.deactivateWith;
        }
    }

    CompositeDisposable? destroyWith;
    protected CompositeDisposable DestroyWith
    {
        get
        {
            this.destroyWith ??= new();
            return this.destroyWith;
        }
    }
    
    CancellationTokenSource? deactiveToken;
    /// <summary>
    /// The destroy cancellation token - called when your model is deactivated
    /// </summary>
    protected CancellationToken DeactivateToken
    {
        get
        {
            this.deactiveToken ??= new CancellationTokenSource();
            return this.deactiveToken.Token;
        }
    }


    CancellationTokenSource? destroyToken;
    /// <summary>
    /// The destroy cancellation token - called when your model is destroyed
    /// </summary>
    protected CancellationToken DestroyToken
    {
        get
        {
            this.destroyToken ??= new CancellationTokenSource();
            return this.destroyToken.Token;
        }
    }
    

    /// <summary>
    /// Will trap any errors - log them and display a message to the user
    /// </summary>
    /// <param name="func"></param>
    /// <param name="markBusy"></param>
    /// <returns></returns>
    protected virtual async Task SafeExecuteAsync(Func<Task> func, bool markBusy = false)
    {
        try
        {
            if (markBusy)
                this.IsBusy = true;

            await func.Invoke().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during safe execution");
            await services.Dialogs.Alert("ERROR", "An unexpected error occurred. Please try again.");
        }
        finally
        {
            this.IsBusy = false;
        }
    }
    

    protected bool IsVisible { get; private set; }
    protected ILogger Logger => field ??= services.LoggerFactory.CreateLogger(this.GetType());
    
    [MainThread]
    public Task Handle(ConnectivityChanged @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        this.IsInternetAvailable = @event.Connected;
        return Task.CompletedTask;
    }


    protected virtual void Deactivate()
    {
        this.deactiveToken?.Cancel();
        this.deactiveToken?.Dispose();
        this.deactiveToken = null;

        this.deactivateWith?.Dispose();
        this.deactivateWith = null;
    }
    

    public virtual void Dispose()
    {
        this.destroyToken?.Cancel();
        this.destroyToken?.Dispose();

        this.Deactivate();
        this.destroyWith?.Dispose();
    }
}