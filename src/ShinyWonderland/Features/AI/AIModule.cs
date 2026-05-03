using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace ShinyWonderland.Features.AI;

public class AIModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        var apiKey = builder.Configuration["GitHubCopilot:ApiKey"] ?? throw new InvalidOperationException("GitHubCopilot ApiKey not found");
        var model = builder.Configuration["GitHubCopilot:Model"] ?? "gpt-4o";

        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.githubcopilot.com")
            }
        );
        var chatClient = client
            .GetChatClient(model)
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        builder.Services.AddSpeechServices();
        builder.Services.AddSingleton(chatClient);
    }

    public void Use(IPlatformApplication app)
    {
    }
}
