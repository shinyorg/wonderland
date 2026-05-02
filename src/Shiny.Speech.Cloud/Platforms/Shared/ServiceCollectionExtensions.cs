#if PLATFORM
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shiny.Speech;
using Shiny.Speech.Cloud;

namespace Shiny;

public static class SpeechCloudServiceCollectionExtensions
{
    /// <summary>
    /// Register cloud-based speech-to-text using a pluggable provider.
    /// This replaces the native ISpeechToTextService with one that captures audio
    /// from the platform microphone and delegates recognition to the provider.
    /// </summary>
    public static IServiceCollection AddCloudSpeechToText<TProvider>(this IServiceCollection services)
        where TProvider : class, ISpeechToTextProvider
    {
        services.AddSingleton<ISpeechToTextProvider, TProvider>();
        services.AddTransient<IAudioSource, AudioSourceImpl>();
        services.AddSingleton<ISpeechToTextService, CloudSpeechToText>();
        return services;
    }

    /// <summary>
    /// Register cloud-based speech-to-text using a provider instance.
    /// </summary>
    public static IServiceCollection AddCloudSpeechToText(this IServiceCollection services, ISpeechToTextProvider provider)
    {
        services.AddSingleton(provider);
        services.AddTransient<IAudioSource, AudioSourceImpl>();
        services.AddSingleton<ISpeechToTextService, CloudSpeechToText>();
        return services;
    }

    /// <summary>
    /// Register cloud-based text-to-speech using a pluggable provider.
    /// This replaces the native ITextToSpeechService with one that synthesizes audio
    /// via the provider and plays it back using the platform audio player.
    /// </summary>
    public static IServiceCollection AddCloudTextToSpeech<TProvider>(this IServiceCollection services)
        where TProvider : class, ITextToSpeechProvider
    {
        services.AddSingleton<ITextToSpeechProvider, TProvider>();
        services.AddSingleton<IAudioPlayer, AudioPlayerImpl>();
        services.AddSingleton<ITextToSpeechService, CloudTextToSpeech>();
        return services;
    }

    /// <summary>
    /// Register cloud-based text-to-speech using a provider instance.
    /// </summary>
    public static IServiceCollection AddCloudTextToSpeech(this IServiceCollection services, ITextToSpeechProvider provider)
    {
        services.AddSingleton(provider);
        services.AddSingleton<IAudioPlayer, AudioPlayerImpl>();
        services.AddSingleton<ITextToSpeechService, CloudTextToSpeech>();
        return services;
    }
}

// Platform-specific aliases resolved by conditional compilation
#if APPLE
file class AudioSourceImpl : AppleAudioSource
{
    public AudioSourceImpl(Microsoft.Extensions.Logging.ILogger<AppleAudioSource> logger) : base(logger) { }
}
file class AudioPlayerImpl : AppleAudioPlayer
{
    public AudioPlayerImpl(Microsoft.Extensions.Logging.ILogger<AppleAudioPlayer> logger) : base(logger) { }
}
#elif ANDROID
file class AudioSourceImpl : AndroidAudioSource
{
    public AudioSourceImpl(Microsoft.Extensions.Logging.ILogger<AndroidAudioSource> logger) : base(logger) { }
}
file class AudioPlayerImpl : AndroidAudioPlayer
{
    public AudioPlayerImpl(Microsoft.Extensions.Logging.ILogger<AndroidAudioPlayer> logger) : base(logger) { }
}
#elif WINDOWS
file class AudioSourceImpl : WindowsAudioSource
{
    public AudioSourceImpl(Microsoft.Extensions.Logging.ILogger<WindowsAudioSource> logger) : base(logger) { }
}
file class AudioPlayerImpl : WindowsAudioPlayer
{
    public AudioPlayerImpl(Microsoft.Extensions.Logging.ILogger<WindowsAudioPlayer> logger) : base(logger) { }
}
#endif
#endif
