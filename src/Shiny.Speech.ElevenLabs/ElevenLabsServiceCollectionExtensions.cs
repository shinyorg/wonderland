#if PLATFORM
using Microsoft.Extensions.DependencyInjection;
using Shiny.Speech.ElevenLabs;

namespace Shiny;

public static class ElevenLabsServiceCollectionExtensions
{
    public static IServiceCollection AddElevenLabsTextToSpeech(this IServiceCollection services, string apiKey)
    {
        services.AddSingleton(new ElevenLabsConfig { ApiKey = apiKey });
        services.AddCloudTextToSpeech<ElevenLabsTextToSpeechProvider>();
        return services;
    }

    public static IServiceCollection AddElevenLabsTextToSpeech(this IServiceCollection services, ElevenLabsConfig config)
    {
        services.AddSingleton(config);
        services.AddCloudTextToSpeech<ElevenLabsTextToSpeechProvider>();
        return services;
    }
}
#endif
