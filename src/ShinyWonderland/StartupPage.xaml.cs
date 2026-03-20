namespace ShinyWonderland;

public partial class StartupPage : ContentPage
{
    public StartupPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    void OnLoaded(object? sender, EventArgs e)
    {
        (BindingContext as StartupViewModel)?.OnLoad();
    }


    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
    }
}