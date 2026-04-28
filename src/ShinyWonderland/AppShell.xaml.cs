namespace ShinyWonderland;


public partial class AppShell : ShinyShell
{
    readonly IMediator mediator;

    public AppShell(StringsLocalized localize, IMediator mediator)
    {
        this.Localize = localize;
        this.mediator = mediator;
        this.BindingContext = this;
        this.InitializeComponent();
    }


    public StringsLocalized Localize { get; }

    [RelayCommand]
    Task AskAI() => this.mediator.Send(new AskAI());
}
