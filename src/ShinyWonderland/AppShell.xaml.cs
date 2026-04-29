namespace ShinyWonderland;


public partial class AppShell : ShinyShell
{
    readonly INavigator navigator;

    public AppShell(StringsLocalized localize, INavigator navigator)
    {
        this.Localize = localize;
        this.navigator = navigator;
        this.BindingContext = this;
        this.InitializeComponent();
    }

    public StringsLocalized Localize { get; }

    [RelayCommand]
    Task AskAI() => navigator.NavigateToAiLoading();
}
