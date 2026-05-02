#if APPLE
using System.Globalization;
using AVFoundation;
using Microsoft.Extensions.Logging;

namespace Shiny.Speech;

public class TextToSpeechImpl(ILogger<TextToSpeechImpl> logger) : ITextToSpeechService
{
    readonly AVSpeechSynthesizer synthesizer = new();

    public bool IsSupported => true;
    public bool IsSpeaking => synthesizer.Speaking;

    public Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        var voices = AVSpeechSynthesisVoice.GetSpeechVoices();
        var results = new List<VoiceInfo>();

        foreach (var voice in voices)
        {
            var voiceCulture = new CultureInfo(voice.Language);
            if (culture == null || voiceCulture.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName)
            {
                results.Add(new VoiceInfo(voice.Identifier, voice.Name, voiceCulture));
            }
        }

        return Task.FromResult<IReadOnlyList<VoiceInfo>>(results);
    }

    public async Task SpeakAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default)
    {
        await StopAsync();
        options ??= new TextToSpeechOptions();

        var utterance = new AVSpeechUtterance(text);

        if (options.Voice != null)
        {
            utterance.Voice = AVSpeechSynthesisVoice.FromIdentifier(options.Voice.Id);
        }
        else if (options.Culture != null)
        {
            utterance.Voice = AVSpeechSynthesisVoice.FromLanguage(options.Culture.Name);
        }

        // AVSpeechUtterance rate: 0.0 (slowest) to 1.0 (fastest), default ~0.5
        utterance.Rate = Math.Clamp(options.SpeechRate * AVSpeechUtterance.DefaultSpeechRate, AVSpeechUtterance.MinimumSpeechRate, AVSpeechUtterance.MaximumSpeechRate);
        utterance.PitchMultiplier = Math.Clamp(options.Pitch, 0.5f, 2.0f);
        utterance.Volume = Math.Clamp(options.Volume, 0f, 1f);

        var tcs = new TaskCompletionSource();

        void OnFinished(object? sender, AVSpeechSynthesizerUteranceEventArgs e)
        {
            if (e.Utterance == utterance)
                tcs.TrySetResult();
        }

        void OnCancelled(object? sender, AVSpeechSynthesizerUteranceEventArgs e)
        {
            if (e.Utterance == utterance)
                tcs.TrySetResult();
        }

        synthesizer.DidFinishSpeechUtterance += OnFinished;
        synthesizer.DidCancelSpeechUtterance += OnCancelled;

        using var reg = cancellationToken.Register(() =>
        {
            synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
        });

        try
        {
            synthesizer.SpeakUtterance(utterance);
            logger.LogDebug("Text-to-speech started");
            await tcs.Task;
            logger.LogDebug("Text-to-speech completed");
        }
        finally
        {
            synthesizer.DidFinishSpeechUtterance -= OnFinished;
            synthesizer.DidCancelSpeechUtterance -= OnCancelled;
        }
    }

    public Task StopAsync()
    {
        if (synthesizer.Speaking)
        {
            synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
            logger.LogDebug("Text-to-speech stopped");
        }
        return Task.CompletedTask;
    }
}
#endif
