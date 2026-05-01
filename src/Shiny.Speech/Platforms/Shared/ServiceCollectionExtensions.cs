#if PLATFORM
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shiny.Speech;

namespace Shiny;

public static class SpeechServiceCollectionExtensions
{
    public static IServiceCollection AddSpeechToText(this IServiceCollection services)
    {
        services.TryAddSingleton<ISpeechToText, SpeechToTextImpl>();
        return services;
    }
}
#endif
