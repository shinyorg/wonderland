namespace ShinyWonderland.Features.AI;

public class AIModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddShinyAiConversation(opts =>
        {
            opts.AddVoiceSelectionTools();
            opts.AddGithubCopilotChatClient();
        });
    }

    public void Use(IPlatformApplication app)
    {
    }
}
