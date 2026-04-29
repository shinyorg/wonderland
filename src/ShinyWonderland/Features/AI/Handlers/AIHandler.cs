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
    ITextToSpeech textToSpeech
) : ICommandHandler<AskAI>
{
    readonly List<ChatMessage> history = [
        new(
            ChatRole.System,
            "You are a helpful theme park assistant. Use the available tools to answer questions about rides, wait times, meals, and park hours."
        ),
        // new(
        //     ChatRole.System,
        //     shellTools.Prompt
        // )
    ];

    public async Task Handle(AskAI command, IMediatorContext context, CancellationToken cancellationToken)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await textToSpeech.SpeakAsync("You are not connected to the internet.", cancelToken: cancellationToken);
            return;
        }

        try
        {
            await textToSpeech.SpeakAsync("What would you like to know?", cancelToken: cancellationToken);

            var granted = await speechToText.RequestPermissions(cancellationToken);
            if (!granted)
                return;

            var tcs = new TaskCompletionSource<string?>();

            void OnCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
                => tcs.TrySetResult(e.RecognitionResult.Text);

            speechToText.RecognitionResultCompleted += OnCompleted;
            try
            {
                await speechToText.StartListenAsync(new CommunityToolkit.Maui.Media.SpeechToTextOptions
                {
                    Culture = System.Globalization.CultureInfo.CurrentCulture,
                    ShouldReportPartialResults = false
                }, cancellationToken);

                await using var reg = cancellationToken.Register(() => tcs.TrySetCanceled());
                var userText = await tcs.Task;
                logger.LogDebug($"User: {userText}");
                
                await speechToText.StopListenAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(userText))
                    return;

                history.Add(new(ChatRole.User, userText));
                var tools = aitools.ToList();
                // tools.AddRange(shellTools.Tools);

                var chatOptions = new ChatOptions
                {
                    Tools = tools
                };

                var response = await chatClient.GetResponseAsync(history, chatOptions, cancellationToken);
                history.AddRange(response.Messages);

                var assistantText = response.Text;
                if (!string.IsNullOrWhiteSpace(assistantText))
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
        catch
        {
            await textToSpeech.SpeakAsync("oh oh - something went wrong");
        }
    }
}
