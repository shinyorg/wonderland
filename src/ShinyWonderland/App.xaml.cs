namespace ShinyWonderland;

public partial class App : Application
{
    readonly StringsLocalized localize;
    
    public App(StringsLocalized localize)
    {
        this.localize = localize;
        this.InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell(this.localize);
        var window = new Window(shell);

        _ = shell.GoToAsync("//startup");
        return window;
    }
}