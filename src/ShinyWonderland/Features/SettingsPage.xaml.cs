namespace ShinyWonderland.Features;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (this.BindingContext is SettingsViewModel vm)
            await vm.LoadVoices();
    }
}
