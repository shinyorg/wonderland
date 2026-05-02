#if ANDROID
using System.Globalization;
using Android.Speech.Tts;
using Microsoft.Extensions.Logging;

namespace Shiny.Speech;

public class TextToSpeechImpl : Java.Lang.Object, ITextToSpeechService, TextToSpeech.IOnInitListener
{
    readonly ILogger<TextToSpeechImpl> logger;
    readonly TaskCompletionSource initTcs = new();
    Android.Speech.Tts.TextToSpeech? tts;
    bool initialized;
    TaskCompletionSource? speakTcs;

    public TextToSpeechImpl(ILogger<TextToSpeechImpl> logger)
    {
        this.logger = logger;
        tts = new Android.Speech.Tts.TextToSpeech(Android.App.Application.Context, this);
    }

    public bool IsSupported => true;
    public bool IsSpeaking => tts?.IsSpeaking ?? false;

    public void OnInit(OperationResult status)
    {
        initialized = status == OperationResult.Success;
        if (initialized)
        {
            logger.LogDebug("Android TTS engine initialized");
            initTcs.TrySetResult();
        }
        else
        {
            logger.LogWarning("Android TTS engine initialization failed: {Status}", status);
            initTcs.TrySetException(new InvalidOperationException($"TTS initialization failed: {status}"));
        }
    }

    async Task EnsureInitializedAsync()
    {
        if (!initialized)
            await initTcs.Task;
    }

    public async Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();

        var voices = tts!.Voices;
        var results = new List<VoiceInfo>();

        if (voices == null)
            return results;

        foreach (var voice in voices)
        {
            var locale = voice.Locale;
            if (locale == null)
                continue;

            var name = voice.Name;
            if (name == null)
                continue;

            var voiceCulture = new CultureInfo(locale.ToLanguageTag());
            if (culture == null || voiceCulture.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName)
            {
                results.Add(new VoiceInfo(name, name, voiceCulture));
            }
        }

        return results;
    }

    public async Task SpeakAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default)
    {
        await StopAsync();
        await EnsureInitializedAsync();

        options ??= new TextToSpeechOptions();

        if (options.Voice != null)
        {
            var voice = tts!.Voices?.FirstOrDefault(v => v.Name == options.Voice.Id);
            if (voice != null)
                tts!.SetVoice(voice);
        }
        else if (options.Culture != null)
        {
            tts!.SetLanguage(Java.Util.Locale.ForLanguageTag(options.Culture.Name));
        }

        // Android TTS: speech rate 1.0 = normal, pitch 1.0 = normal
        tts!.SetSpeechRate(Math.Max(0.1f, options.SpeechRate));
        tts!.SetPitch(Math.Max(0.1f, options.Pitch));

        speakTcs = new TaskCompletionSource();
        var utteranceId = Guid.NewGuid().ToString();
        var listener = new UtteranceListener(speakTcs, logger);
        tts!.SetOnUtteranceProgressListener(listener);

        var bundle = new Android.OS.Bundle();
        bundle.PutFloat(Android.Speech.Tts.TextToSpeech.Engine.KeyParamVolume, Math.Clamp(options.Volume, 0f, 1f));

        using var reg = cancellationToken.Register(() =>
        {
            tts!.Stop();
            speakTcs?.TrySetResult();
        });

        tts!.Speak(text, QueueMode.Flush, bundle, utteranceId);
        logger.LogDebug("Text-to-speech started");

        await speakTcs.Task;
        logger.LogDebug("Text-to-speech completed");
    }

    public Task StopAsync()
    {
        if (tts?.IsSpeaking == true)
        {
            tts.Stop();
            speakTcs?.TrySetResult();
            logger.LogDebug("Text-to-speech stopped");
        }
        return Task.CompletedTask;
    }

    sealed class UtteranceListener(TaskCompletionSource tcs, ILogger logger) : UtteranceProgressListener
    {
        public override void OnDone(string? utteranceId) => tcs.TrySetResult();

        public override void OnError(string? utteranceId)
        {
            logger.LogWarning("TTS utterance error: {UtteranceId}", utteranceId);
            tcs.TrySetException(new InvalidOperationException("Text-to-speech utterance failed"));
        }

        public override void OnStart(string? utteranceId)
            => logger.LogDebug("TTS utterance started: {UtteranceId}", utteranceId);
    }
}
#endif
