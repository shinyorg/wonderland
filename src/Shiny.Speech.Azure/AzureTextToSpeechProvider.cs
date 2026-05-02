using System.Globalization;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Shiny.Speech.Cloud;
using AzureVoiceInfo = Microsoft.CognitiveServices.Speech.VoiceInfo;

namespace Shiny.Speech.Azure;

public class AzureTextToSpeechProvider(
    AzureSpeechConfig config,
    ILogger<AzureTextToSpeechProvider> logger
) : ITextToSpeechProvider
{
    public async Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        var speechConfig = SpeechConfig.FromSubscription(config.SubscriptionKey, config.Region);
        using var synthesizer = new SpeechSynthesizer(speechConfig, null);

        var voicesResult = await synthesizer.GetVoicesAsync(culture?.Name ?? "");
        if (voicesResult.Reason == ResultReason.VoicesListRetrieved)
        {
            return voicesResult.Voices
                .Select(v => new VoiceInfo(v.ShortName, v.LocalName, new CultureInfo(v.Locale)))
                .ToList();
        }

        logger.LogWarning("Failed to retrieve Azure voices: {Reason}", voicesResult.Reason);
        return [];
    }

    public async Task<Stream> SynthesizeAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new TextToSpeechOptions();

        var speechConfig = SpeechConfig.FromSubscription(config.SubscriptionKey, config.Region);
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

        if (options.Voice != null)
            speechConfig.SpeechSynthesisVoiceName = options.Voice.Id;
        else if (options.Culture != null)
            speechConfig.SpeechSynthesisLanguage = options.Culture.Name;

        using var synthesizer = new SpeechSynthesizer(speechConfig, null);

        // Build SSML for rate/pitch/volume control
        var voiceName = options.Voice?.Id
            ?? speechConfig.SpeechSynthesisVoiceName
            ?? "en-US-AriaNeural";

        var ratePercent = ((options.SpeechRate - 1.0f) * 100).ToString("+0;-0;+0");
        var pitchPercent = ((options.Pitch - 1.0f) * 100).ToString("+0;-0;+0");
        var volumeValue = (int)(options.Volume * 100);

        var ssml = $"""
            <speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="{options.Culture?.Name ?? "en-US"}">
                <voice name="{voiceName}">
                    <prosody rate="{ratePercent}%" pitch="{pitchPercent}%" volume="{volumeValue}">
                        {System.Security.SecurityElement.Escape(text)}
                    </prosody>
                </voice>
            </speak>
            """;

        var result = await synthesizer.SpeakSsmlAsync(ssml);

        if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            logger.LogError("Azure TTS canceled: {Reason} {ErrorDetails}", cancellation.Reason, cancellation.ErrorDetails);
            throw new InvalidOperationException($"Azure TTS failed: {cancellation.ErrorDetails}");
        }

        logger.LogDebug("Azure TTS synthesis completed, {Bytes} bytes", result.AudioData.Length);
        return new MemoryStream(result.AudioData);
    }
}
