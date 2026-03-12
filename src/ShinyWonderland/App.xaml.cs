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
        => new(new StartAppShell());
}