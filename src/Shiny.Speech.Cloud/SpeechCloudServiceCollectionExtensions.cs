using Microsoft.Extensions.DependencyInjection;
using Shiny.Speech;
using Shiny.Speech.Cloud;

namespace Shiny;

public static class SpeechCloudServiceCollectionExtensions
{
    /// <summary>
    /// Register cloud-based speech-to-text using a pluggable provider.
    /// Requires IAudioSource to be registered (call AddAudioSource() from Shiny.Speech).
    /// </summary>
    public static IServiceCollection AddCloudSpeechToText<TProvider>(this IServiceCollection services)
        where TProvider : class, ISpeechToTextProvider
    {
        services.AddSingleton<ISpeechToTextProvider, TProvider>();
        services.AddSingleton<ISpeechToTextService, CloudSpeechToText>();
        return services;
    }

    /// <summary>
    /// Register cloud-based speech-to-text using a provider instance.
    /// Requires IAudioSource to be registered (call AddAudioSource() from Shiny.Speech).
    /// </summary>
    public static IServiceCollection AddCloudSpeechToText(this IServiceCollection services, ISpeechToTextProvider provider)
    {
        services.AddSingleton(provider);
        services.AddSingleton<ISpeechToTextService, CloudSpeechToText>();
        return services;
    }

    /// <summary>
    /// Register cloud-based text-to-speech using a pluggable provider.
    /// Requires IAudioPlayer to be registered (call AddAudioPlayer() from Shiny.Speech).
    /// </summary>
    public static IServiceCollection AddCloudTextToSpeech<TProvider>(this IServiceCollection services)
        where TProvider : class, ITextToSpeechProvider
    {
        services.AddSingleton<ITextToSpeechProvider, TProvider>();
        services.AddSingleton<ITextToSpeechService, CloudTextToSpeech>();
        return services;
    }

    /// <summary>
    /// Register cloud-based text-to-speech using a provider instance.
    /// Requires IAudioPlayer to be registered (call AddAudioPlayer() from Shiny.Speech).
    /// </summary>
    public static IServiceCollection AddCloudTextToSpeech(this IServiceCollection services, ITextToSpeechProvider provider)
    {
        services.AddSingleton(provider);
        services.AddSingleton<ITextToSpeechService, CloudTextToSpeech>();
        return services;
    }
}
