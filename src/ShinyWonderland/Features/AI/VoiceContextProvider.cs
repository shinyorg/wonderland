using Shiny.AiConversation;
using Shiny.Speech;

namespace ShinyWonderland.Features.AI;

[Singleton]
public class VoiceContextProvider(AppSettings settings, ITextToSpeechService tts) : IContextProvider
{
    VoiceInfo? cachedVoice;
    string? cachedVoiceId;

    public async Task Apply(AiContext context)
    {
        var existing = context.TextToSpeechOptions;
        var voice = existing?.Voice;

        if (!string.IsNullOrWhiteSpace(settings.VoiceId))
        {
            if (cachedVoice == null || cachedVoiceId != settings.VoiceId)
            {
                var voices = await tts.GetVoicesAsync();
                cachedVoice = voices.FirstOrDefault(v => v.Id == settings.VoiceId);
                cachedVoiceId = settings.VoiceId;
            }
            voice = cachedVoice ?? voice;
        }

        context.TextToSpeechOptions = new TextToSpeechOptions
        {
            Voice = voice,
            Culture = existing?.Culture,
            Volume = existing?.Volume ?? 1f,
            SpeechRate = settings.SpeechRatePercent / 100f,
            Pitch = settings.PitchPercent / 100f
        };
    }
}
