using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.AI;

namespace ShinyWonderland.Features.AI.Handlers;

public record AskAI : ICommand;

[MediatorSingleton]
public partial class AIHandler(
    ILogger<AIHandler> logger,
    IChatClient chatClient,
    AiMauiShellTools shellTools,
    IEnumerable<AITool> aitools,
    ISpeechToText speechToText,
    ITextToSpeech textToSpeech,
    IMediator mediator
) : ICommandHandler<AskAI>
{
    readonly List<ChatMessage> SystemMessages = [
        new(
            ChatRole.System,
            "You are a helpful theme park assistant. Use the available tools to answer questions about rides, wait times, meals, and park hours."
        ),
        new(
            ChatRole.System,
            shellTools.Prompt
        )
    ];

    Task SendPhase(AiPhase phase, CancellationToken ct)
        => mediator.Publish(new AiPhaseChanged(phase), ct);

    public async Task Handle(AskAI command, IMediatorContext context, CancellationToken cancellationToken)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await SendPhase(AiPhase.Speaking, cancellationToken);
            await textToSpeech.SpeakAsync("You are not connected to the internet.", cancelToken: cancellationToken);
            return;
        }

        try
        {
            var granted = await speechToText.RequestPermissions(cancellationToken);
            if (!granted)
                return;

            var tcs = new TaskCompletionSource<string?>();
            void OnCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
                => tcs.TrySetResult(e.RecognitionResult.Text);

            speechToText.RecognitionResultCompleted += OnCompleted;
            try
            {
                await SendPhase(AiPhase.Prompting, cancellationToken);

                await speechToText.StartListenAsync(new CommunityToolkit.Maui.Media.SpeechToTextOptions
                {
                    Culture = System.Globalization.CultureInfo.CurrentCulture,
                    ShouldReportPartialResults = false
                }, cancellationToken);

                await textToSpeech.SpeakAsync("What would you like to know?", cancelToken: cancellationToken);

                await SendPhase(AiPhase.Listening, cancellationToken);

                await using var reg = cancellationToken.Register(() => tcs.TrySetCanceled());
                var userText = await tcs.Task;
                logger.LogDebug($"User: {userText}");

                await speechToText.StopListenAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(userText))
                    return;

                await SendPhase(AiPhase.Thinking, cancellationToken);

                var tools = aitools.ToList();
                tools.AddRange(shellTools.Tools);

                var copy = this.SystemMessages.ToList();
                copy.Add(new ChatMessage(ChatRole.User, userText));
                var chatOptions = new ChatOptions
                {
                    Tools = tools
                };

                var response = await chatClient.GetResponseAsync(copy, chatOptions, cancellationToken);
                var assistantText = response.Text;
                logger.LogDebug($"Assistant: {assistantText}");

                if (String.IsNullOrWhiteSpace(assistantText))
                    assistantText = "I have nothing useful to say";

                await SendPhase(AiPhase.Speaking, cancellationToken);
                await textToSpeech.SpeakAsync(assistantText, cancelToken: cancellationToken);
            }
            finally
            {
                speechToText.RecognitionResultCompleted -= OnCompleted;
            }
        }
        catch (OperationCanceledException)
        {
            // user cancelled — nothing to do
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI handler failed");
            await SendPhase(AiPhase.Speaking, CancellationToken.None);
            await textToSpeech.SpeakAsync("oh oh - something went wrong");
        }
    }
}
