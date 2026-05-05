using Microsoft.Extensions.AI;
using Shiny.Speech;

namespace ShinyWonderland.Features.AI.Handlers;

public record AskAI : ICommand;

[MediatorSingleton]
public partial class AIHandler(
    ILogger<AIHandler> logger,
    IChatClient chatClient,
    AiMauiShellTools shellTools,
    IEnumerable<AITool> aitools,
    ISpeechToTextService speechToText,
    ITextToSpeechService textToSpeech,
    IMediator mediator,
    TimeProvider timeProvider
) : ICommandHandler<AskAI>
{
    string? rideListPrompt;

    async Task<List<ChatMessage>> GetSystemMessages(IMediatorContext context, CancellationToken ct)
    {
        if (rideListPrompt == null)
        {
            try
            {
                var rides = await context.Request(new Features.Rides.Pages.GetCurrentRideTimes(), ct);
                var lines = rides
                    .OrderBy(r => r.Name)
                    .Select(r => $"- {r.Name} (ID: {r.Id})");
                rideListPrompt = "Here are all the rides in the park with their IDs. Always use the ride ID when calling tools:\n" + string.Join('\n', lines);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch ride list for system prompt");
                rideListPrompt = "";
            }
        }

        var now = timeProvider.GetLocalNow();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful theme park assistant. Use the available tools to answer questions about rides, wait times, meals, and park hours."),
            new(ChatRole.System, $"The current date and time is {now:dddd, MMMM d, yyyy h:mm tt}."),
            new(ChatRole.System, shellTools.Prompt)
        };

        if (!string.IsNullOrEmpty(rideListPrompt))
            messages.Add(new(ChatRole.System, rideListPrompt));

        return messages;
    }

    Task SendPhase(AiPhase phase, CancellationToken ct)
        => mediator.Publish(new AiPhaseChanged(phase), ct);

    public async Task Handle(AskAI command, IMediatorContext context, CancellationToken cancellationToken)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await SendPhase(AiPhase.Speaking, cancellationToken);
            await textToSpeech.SpeakAsync("You are not connected to the internet.", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            var access = await speechToText.RequestAccess();
            if (access != Shiny.Speech.AccessState.Available)
                return;

            await SendPhase(AiPhase.Prompting, cancellationToken);
            await textToSpeech.SpeakAsync("What would you like to know?", cancellationToken: cancellationToken);

            // allow audio session to fully release before starting speech recognition
            await Task.Delay(500, cancellationToken);

            await SendPhase(AiPhase.Listening, cancellationToken);

            var userText = await speechToText.ListenUntilSilence(cancellationToken: cancellationToken);
            logger.LogDebug($"User: {userText}");
            if (string.IsNullOrWhiteSpace(userText))
                return;

            await textToSpeech.SpeakAsync("Thinking...",  cancellationToken: cancellationToken);
            await SendPhase(AiPhase.Thinking, cancellationToken);
            
            var tools = aitools.ToList();
            tools.AddRange(shellTools.Tools);

            var copy = await GetSystemMessages(context, cancellationToken);
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
            await textToSpeech.SpeakAsync(assistantText, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // user cancelled — nothing to do
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI handler failed");
            await SendPhase(AiPhase.Speaking, CancellationToken.None);
            await textToSpeech.SpeakAsync("oh oh - something went wrong", cancellationToken: cancellationToken);
        }
    }
}
