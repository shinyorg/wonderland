using Microsoft.Extensions.AI;
using Shiny.AiConversation;

namespace ShinyWonderland.Delegates;

[Singleton]
public class ShellAiContextProvider(AiMauiShellTools tools) : IContextProvider
{
    public IEnumerable<string> GetSystemPrompts(AiAcknowledgement acknowledgement)
        => [tools.Prompt];

    public IEnumerable<AITool> GetTools()
        => tools.Tools;
}