namespace ShinyWonderland;


public partial class AppShell : Shell
{
    public AppShell(StringsLocalized localize)
    {
        this.InitializeComponent();
        this.Localize = localize;
    }
    
    
    public StringsLocalized Localize { get; }
}