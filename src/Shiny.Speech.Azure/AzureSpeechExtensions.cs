#if PLATFORM
using Microsoft.Extensions.DependencyInjection;
using Shiny.Speech.Azure;

namespace Shiny;

public static class AzureSpeechExtensions
{
    /// <summary>
    /// Register Azure AI Speech for both speech-to-text and text-to-speech.
    /// </summary>
    public static IServiceCollection AddAzureSpeech(
        this IServiceCollection services,
        string subscriptionKey,
        string region,
        bool speechToText = true,
        bool textToSpeech = true
    )
    {
        var config = new AzureSpeechConfig
        {
            SubscriptionKey = subscriptionKey,
            Region = region
        };
        return services.AddAzureSpeech(config, speechToText, textToSpeech);
    }

    /// <summary>
    /// Register Azure AI Speech for both speech-to-text and text-to-speech.
    /// </summary>
    public static IServiceCollection AddAzureSpeech(
        this IServiceCollection services,
        AzureSpeechConfig config,
        bool speechToText = true,
        bool textToSpeech = true
    )
    {
        services.AddSingleton(config);

        if (speechToText)
            services.AddCloudSpeechToText<AzureSpeechToTextProvider>();

        if (textToSpeech)
            services.AddCloudTextToSpeech<AzureTextToSpeechProvider>();

        return services;
    }
}
#endif
