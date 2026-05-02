#if PLATFORM
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shiny.Speech;

namespace Shiny;

public static class SpeechServiceCollectionExtensions
{
    public static IServiceCollection AddSpeechToText(this IServiceCollection services)
    {
        services.TryAddSingleton<ISpeechToTextService, SpeechToTextImpl>();
        return services;
    }

    public static IServiceCollection AddTextToSpeech(this IServiceCollection services)
    {
        services.TryAddSingleton<ITextToSpeechService, TextToSpeechImpl>();
        return services;
    }
}
#endif
