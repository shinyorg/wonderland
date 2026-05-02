#if WINDOWS
using System.Globalization;
using Microsoft.Extensions.Logging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace Shiny.Speech;

public class TextToSpeechImpl(ILogger<TextToSpeechImpl> logger) : ITextToSpeechService, IDisposable
{
    readonly SpeechSynthesizer synthesizer = new();
    MediaPlayer? mediaPlayer;

    public bool IsSupported => true;
    public bool IsSpeaking => mediaPlayer?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing;

    public Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        var voices = SpeechSynthesizer.AllVoices;
        var results = new List<VoiceInfo>();

        foreach (var voice in voices)
        {
            var voiceCulture = new CultureInfo(voice.Language);
            if (culture == null || voiceCulture.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName)
            {
                results.Add(new VoiceInfo(voice.Id, voice.DisplayName, voiceCulture));
            }
        }

        return Task.FromResult<IReadOnlyList<VoiceInfo>>(results);
    }

    public async Task SpeakAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default)
    {
        await StopAsync();
        options ??= new TextToSpeechOptions();

        if (options.Voice != null)
        {
            var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Id == options.Voice.Id);
            if (voice != null)
                synthesizer.Voice = voice;
        }
        else if (options.Culture != null)
        {
            var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v =>
                new CultureInfo(v.Language).TwoLetterISOLanguageName == options.Culture.TwoLetterISOLanguageName);
            if (voice != null)
                synthesizer.Voice = voice;
        }

        synthesizer.Options.SpeakingRate = Math.Clamp((double)options.SpeechRate, 0.5, 6.0);
        synthesizer.Options.AudioPitch = Math.Clamp((double)options.Pitch, 0.5, 2.0);
        synthesizer.Options.AudioVolume = Math.Clamp((double)options.Volume, 0.0, 1.0);

        var stream = await synthesizer.SynthesizeTextToStreamAsync(text);
        var tcs = new TaskCompletionSource();

        mediaPlayer = new MediaPlayer();
        mediaPlayer.Source = MediaSource.CreateFromStream(stream, stream.ContentType);

        mediaPlayer.MediaEnded += (_, _) => tcs.TrySetResult();
        mediaPlayer.MediaFailed += (_, args) =>
        {
            logger.LogWarning("TTS media playback failed: {Error}", args.ErrorMessage);
            tcs.TrySetException(new InvalidOperationException($"TTS playback failed: {args.ErrorMessage}"));
        };

        using var reg = cancellationToken.Register(() =>
        {
            mediaPlayer.Pause();
            tcs.TrySetResult();
        });

        mediaPlayer.Play();
        logger.LogDebug("Text-to-speech started");

        await tcs.Task;
        logger.LogDebug("Text-to-speech completed");

        mediaPlayer.Dispose();
        mediaPlayer = null;
    }

    public Task StopAsync()
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.Pause();
            mediaPlayer.Dispose();
            mediaPlayer = null;
            logger.LogDebug("Text-to-speech stopped");
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        mediaPlayer?.Dispose();
        synthesizer.Dispose();
    }
}
#endif
