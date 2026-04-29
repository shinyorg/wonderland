namespace ShinyWonderland;

public partial class App : Application
{
    readonly StringsLocalized localize;
    readonly INavigator navigator;

    public App(StringsLocalized localize, INavigator navigator)
    {
        this.localize = localize;
        this.navigator = navigator;
        this.InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell(this.localize, this.navigator);
        return new Window(shell);
    }
}