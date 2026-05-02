using Microsoft.Extensions.DependencyInjection;
using Shiny.Speech.ElevenLabs;

namespace Shiny;

public static class ElevenLabsServiceCollectionExtensions
{
    /// <summary>
    /// Register ElevenLabs text-to-speech.
    /// Requires IAudioPlayer to be registered (call AddAudioPlayer() from Shiny.Speech).
    /// </summary>
    public static IServiceCollection AddElevenLabsTextToSpeech(this IServiceCollection services, string apiKey)
    {
        services.AddSingleton(new ElevenLabsConfig { ApiKey = apiKey });
        services.AddCloudTextToSpeech<ElevenLabsTextToSpeechProvider>();
        return services;
    }

    /// <summary>
    /// Register ElevenLabs text-to-speech.
    /// Requires IAudioPlayer to be registered (call AddAudioPlayer() from Shiny.Speech).
    /// </summary>
    public static IServiceCollection AddElevenLabsTextToSpeech(this IServiceCollection services, ElevenLabsConfig config)
    {
        services.AddSingleton(config);
        services.AddCloudTextToSpeech<ElevenLabsTextToSpeechProvider>();
        return services;
    }
}
