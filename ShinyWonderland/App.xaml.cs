namespace ShinyWonderland;

public partial class App : Application
{
    readonly AppShellLocalized localize;
    
    public App(AppShellLocalized localize)
    {
        this.localize = localize;
        this.InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new AppShell(this.localize));
}