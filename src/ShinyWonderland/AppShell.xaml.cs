namespace ShinyWonderland;


public partial class AppShell : ShinyShell
{
    public AppShell(StringsLocalized localize)
    {
        this.Localize = localize;
        this.BindingContext = this;
        this.InitializeComponent();
    }

    public StringsLocalized Localize { get; }
}
