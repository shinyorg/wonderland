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
    /// This replaces the native ISpeechToText with one that captures audio
    /// from the platform microphone and delegates recognition to the provider.
    /// </summary>
    public static IServiceCollection AddCloudSpeechToText<TProvider>(this IServiceCollection services)
        where TProvider : class, ISpeechToTextProvider
    {
        services.AddSingleton<ISpeechToTextProvider, TProvider>();
        services.AddTransient<IAudioSource, AudioSourceImpl>();
        services.AddSingleton<ISpeechToText, CloudSpeechToText>();
        return services;
    }

    /// <summary>
    /// Register cloud-based speech-to-text using a provider instance.
    /// </summary>
    public static IServiceCollection AddCloudSpeechToText(this IServiceCollection services, ISpeechToTextProvider provider)
    {
        services.AddSingleton(provider);
        services.AddTransient<IAudioSource, AudioSourceImpl>();
        services.AddSingleton<ISpeechToText, CloudSpeechToText>();
        return services;
    }
}

// Platform-specific alias resolved by conditional compilation
#if APPLE
file class AudioSourceImpl : AppleAudioSource
{
    public AudioSourceImpl(Microsoft.Extensions.Logging.ILogger<AppleAudioSource> logger) : base(logger) { }
}
#elif ANDROID
file class AudioSourceImpl : AndroidAudioSource
{
    public AudioSourceImpl(Microsoft.Extensions.Logging.ILogger<AndroidAudioSource> logger) : base(logger) { }
}
#elif WINDOWS
file class AudioSourceImpl : WindowsAudioSource
{
    public AudioSourceImpl(Microsoft.Extensions.Logging.ILogger<WindowsAudioSource> logger) : base(logger) { }
}
#endif
#endif
