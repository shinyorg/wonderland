using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

namespace ShinyWonderland.Features.AI;

public class AIModule : IMauiModule
{
    public void Add(MauiAppBuilder builder)
    {
        builder.Services.AddChatClient(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var apiKey = config["GitHubCopilot:ApiKey"] ?? "";
            var model = config["GitHubCopilot:Model"] ?? "gpt-4o";

            var client = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.githubcopilot.com")
                }
            );
            return client.GetChatClient(model).AsIChatClient();
        });
    }

    public void Use(IPlatformApplication app)
    {
    }
}
