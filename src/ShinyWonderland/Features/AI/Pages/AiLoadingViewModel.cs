namespace ShinyWonderland.Features.AI.Pages;

[ShellMap<AiLoadingPage>]
public partial class AiLoadingViewModel(ViewModelServices services) : BaseViewModel(services)
{
    [RelayCommand]
    void Cancel() => this.Deactivate();
    
    public override async void OnAppearing()
    {
        await Mediator.Send(new AskAI(), this.DeactivateToken);
        await Navigator.GoBack();
    }
}