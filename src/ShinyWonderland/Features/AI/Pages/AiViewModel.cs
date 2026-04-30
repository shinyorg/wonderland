namespace ShinyWonderland.Features.AI.Pages;

[ShellMap<AiPage>(registerRoute: false)]
public partial class AiViewModel(ViewModelServices services) :
    BaseViewModel(services),
    IEventHandler<AiPhaseChanged>
{
    public event Action<AiPhase>? PhaseChanged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    AiPhase currentPhase = AiPhase.Idle;

    public bool IsActive => CurrentPhase != AiPhase.Idle;

    [RelayCommand]
    async Task ToggleAi()
    {
        if (this.IsActive)
        {
            this.Deactivate();
            this.CurrentPhase = AiPhase.Idle;
            PhaseChanged?.Invoke(AiPhase.Idle);
            return;
        }

        try
        {
            this.CurrentPhase = AiPhase.Prompting;
            PhaseChanged?.Invoke(AiPhase.Prompting);
            await Mediator.Send(new AskAI(), this.DeactivateToken);
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred during AI interaction");
        }
        finally
        {
            this.CurrentPhase = AiPhase.Idle;
            PhaseChanged?.Invoke(AiPhase.Idle);
        }
    }

    [MainThread]
    public Task Handle(AiPhaseChanged @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        this.CurrentPhase = @event.Phase;
        PhaseChanged?.Invoke(@event.Phase);
        return Task.CompletedTask;
    }
}
