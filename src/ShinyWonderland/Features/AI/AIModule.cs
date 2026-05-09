using Shiny.AiConversation;

namespace ShinyWonderland.Features.AI;

public class AIModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddShinyAiConversation(opts =>
        {
            var apiKey = builder.Configuration["GitHubCopilot:ApiKey"] ?? throw new InvalidOperationException("GitHubCopilot ApiKey not found");
            var model = builder.Configuration["GitHubCopilot:Model"] ?? "gpt-4o";

            opts.AddStaticGithubCopilotChatClient(apiKey, model);
        });
    }

    public void Use(IPlatformApplication app)
    {
        var aiService = app.Services.GetRequiredService<IAiConversationService>();

        // aiService.SystemPrompts.Add(
        //     "You are a helpful theme park assistant for Canada's Wonderland. " +
        //     "Use the available tools to answer questions about rides, wait times, meals, weather, and park hours. " +
        //     "When asked about rides, use the GetCurrentRideTimes tool to look them up. " +
        //     "Always use the ride ID when calling ride-related tools."
        // );
        //
        // var shellTools = app.Services.GetRequiredService<AiMauiShellTools>();
        // aiService.SystemPrompts.Add(shellTools.Prompt);
        //
        // aiService.Acknowledgement = AiAcknowledgement.LessWordy;
    }
}
