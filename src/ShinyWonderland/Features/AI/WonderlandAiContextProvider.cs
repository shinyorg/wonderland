using Shiny.AiConversation;

namespace ShinyWonderland.Features.AI;

[Singleton]
public class ShellAiContextProvider(AiMauiShellTools tools, IMediator mediator) : IContextProvider
{
    const string DEFAULT_PROMPT =
        "You are a helpful theme park assistant for Canada's Wonderland. " +
        "Use the available tools to answer questions about rides, wait times, meals, weather, and park hours. " +
        "When asked about rides, use the GetCurrentRideTimes tool to look them up. " +
        "Always use the ride ID when calling ride-related tools.";


    public async Task Apply(AiContext context)
    {
        context.SystemPrompts.AddRange(DEFAULT_PROMPT, tools.Prompt);
        context.Tools.AddRange(tools.Tools);

        var rides = await mediator.Request(new GetParkRidesRequest());
        if (rides.Result.Count > 0)
        {
            var rideList = string.Join("\n", rides.Result.Select(r => $"- {r.Name} (ID: {r.Id})"));
            context.SystemPrompts.Add($"Here are the available rides and their IDs:\n{rideList}");
        }
    }
}