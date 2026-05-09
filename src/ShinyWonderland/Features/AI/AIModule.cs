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
    }
}
