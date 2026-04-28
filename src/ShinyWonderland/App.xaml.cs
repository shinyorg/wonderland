using Shiny.Mediator;

namespace ShinyWonderland;

public partial class App : Application
{
    readonly StringsLocalized localize;
    readonly IMediator mediator;

    public App(StringsLocalized localize, IMediator mediator)
    {
        this.localize = localize;
        this.mediator = mediator;
        this.InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell(this.localize, this.mediator);
        return new Window(shell);
    }
}