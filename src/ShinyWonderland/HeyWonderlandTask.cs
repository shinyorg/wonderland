#if IOS
using Shiny.Speech;

namespace ShinyWonderland;

[Singleton]
public class HeyWonderTask(
    AppSettings appsettings,
    ISpeechToTextService speechToTextService,
    IMediator mediator,
    ILogger<HeyWonderTask> logger
) : IMauiInitializeService
{
    CancellationTokenSource? cts;

    public void Initialize(IServiceProvider services)
    {
        appsettings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppSettings.IsHeyWonderlandEnabled))
            {
                if (appsettings.IsHeyWonderlandEnabled)
                    Start();
                else
                    Stop();
            }
        };

        if (appsettings.IsHeyWonderlandEnabled)
            Start();
    }

    void Start()
    {
        Stop();
        cts = new CancellationTokenSource();
        _ = ListenLoop(cts.Token);
    }

    void Stop()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        mediator.Publish(new AiPhaseChanged(AiPhase.Idle));
    }

    async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await mediator.Publish(new AiPhaseChanged(AiPhase.Listening), ct);
                var userText = await speechToTextService.ListenWithWakeWord("Hey Wonderland", cancellationToken: ct);
                await mediator.Send(new AskAI(userText), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hey Wonderland listener error");
            }
        }
    }
}
#endif