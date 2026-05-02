using System.Globalization;
using Microsoft.Extensions.Logging;
using Shiny.Speech;

namespace Shiny.Speech.Cloud;

/// <summary>
/// ITextToSpeechService implementation that delegates synthesis to a pluggable ITextToSpeechProvider
/// and plays back the resulting audio using a platform-specific IAudioPlayer.
/// </summary>
public class CloudTextToSpeech(
    ITextToSpeechProvider provider,
    IAudioPlayer audioPlayer,
    ILogger<CloudTextToSpeech> logger
) : ITextToSpeechService
{
    public bool IsSupported => true;
    public bool IsSpeaking => audioPlayer.IsPlaying;

    public Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
        => provider.GetVoicesAsync(culture, cancellationToken);

    public async Task SpeakAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default)
    {
        await StopAsync();

        logger.LogDebug("Synthesizing speech via cloud provider");
        var audioStream = await provider.SynthesizeAsync(text, options, cancellationToken);

        logger.LogDebug("Playing synthesized audio");
        await audioPlayer.PlayAsync(audioStream, cancellationToken);

        logger.LogDebug("Cloud text-to-speech completed");
    }

    public Task StopAsync() => audioPlayer.StopAsync();
}
