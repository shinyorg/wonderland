using Microsoft.Extensions.AI;
using Shiny.AiConversation;

namespace ShinyWonderland.Features.AI;

[Singleton]
public class ShellAiContextProvider(AiMauiShellTools tools) : IContextProvider
{
    public IEnumerable<string> GetSystemPrompts(AiAcknowledgement acknowledgement)
        => [
            "You are a helpful theme park assistant for Canada's Wonderland. " +
            "Use the available tools to answer questions about rides, wait times, meals, weather, and park hours. " +
            "When asked about rides, use the GetCurrentRideTimes tool to look them up. " +
            "Always use the ride ID when calling ride-related tools.",
            tools.Prompt
        ];

    public IEnumerable<AITool> GetTools()
        => tools.Tools;
}