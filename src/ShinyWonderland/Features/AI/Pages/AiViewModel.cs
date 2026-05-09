using Shiny.AiConversation;

namespace ShinyWonderland.Features.AI.Pages;

[ShellMap<AiPage>(registerRoute: false)]
public partial class AiViewModel(
    ViewModelServices services,
    IAiConversationService aiService
) : BaseViewModel(services)
{
    public event Action<AiState>? StateChanged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    AiState currentState = AiState.Idle;

    public bool IsActive => CurrentState != AiState.Idle;

    public override void OnAppearing()
    {
        base.OnAppearing();
        aiService.StatusChanged += OnServiceStateChanged;
    }

    public override void OnDisappearing()
    {
        base.OnDisappearing();
        aiService.StatusChanged -= OnServiceStateChanged;
    }

    void OnServiceStateChanged(AiState state)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentState = state;
            StateChanged?.Invoke(state);
        });
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    async Task ToggleAi()
    {
        if (this.IsActive)
        {
            this.Deactivate();
            return;
        }

        try
        {
            await aiService.ListenAndTalk(this.DeactivateToken);
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred during AI interaction");
        }
    }
}
