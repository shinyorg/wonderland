namespace ShinyWonderland.Features.AI.Pages;

[ShellMap<AiLoadingPage>]
public partial class AiLoadingViewModel(ViewModelServices services) : BaseViewModel(services),
    IEventHandler<AiPhaseChanged>
{
    public event Action<AiPhase>? PhaseChanged;

    [RelayCommand]
    void Cancel() => this.Deactivate();

    public override async void OnAppearing()
    {
        await Mediator.Send(new AskAI(), this.DeactivateToken);
        await Navigator.GoBack();
    }

    [MainThread]
    public Task Handle(AiPhaseChanged @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        PhaseChanged?.Invoke(@event.Phase);
        return Task.CompletedTask;
    }
}
