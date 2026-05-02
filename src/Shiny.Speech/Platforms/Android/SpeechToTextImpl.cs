#if ANDROID
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Speech;
using Microsoft.Extensions.Logging;

namespace Shiny.Speech;

public class SpeechToTextImpl(ILogger<SpeechToTextImpl> logger) : ISpeechToTextService
{
    public bool IsSupported =>
        Android.Speech.SpeechRecognizer.IsRecognitionAvailable(Android.App.Application.Context);

    public Task<AccessState> RequestAccess()
    {
        if (!IsSupported)
            return Task.FromResult(AccessState.NotSupported);

        var context = Android.App.Application.Context;
        var result = context.CheckSelfPermission(Manifest.Permission.RecordAudio);

        return Task.FromResult(result == Permission.Granted
            ? AccessState.Available
            : AccessState.Denied);
    }

    public async IAsyncEnumerable<SpeechRecognitionResult> ContinuousRecognize(
        SpeechRecognitionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new SpeechRecognitionOptions();

        var channel = Channel.CreateUnbounded<SpeechRecognitionResult>(new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = true
        });

        Android.Speech.SpeechRecognizer? recognizer = null;
        var listener = new SpeechListener(channel.Writer, logger);
        var handler = new Handler(Looper.MainLooper!);

        try
        {
            var tcs = new TaskCompletionSource();
            handler.Post(() =>
            {
                recognizer = Android.Speech.SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
                recognizer.SetRecognitionListener(listener);

                var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
                intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                if (options.Culture != null)
                    intent.PutExtra(RecognizerIntent.ExtraLanguage, options.Culture.Name);

                var silenceMs = (long)options.SilenceTimeout.TotalMilliseconds;
                intent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, silenceMs);
                intent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, silenceMs);
                intent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, silenceMs);

                recognizer.StartListening(intent);
                logger.LogDebug("Android speech recognition started");
                tcs.SetResult();
            });
            await tcs.Task;

            using var reg = cancellationToken.Register(() =>
            {
                handler.Post(() =>
                {
                    recognizer?.StopListening();
                    channel.Writer.TryComplete();
                });
            });

            await foreach (var result in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                yield return result;
            }
        }
        finally
        {
            var r = recognizer;
            if (r != null)
            {
                handler.Post(() => r.Destroy());
            }
            logger.LogDebug("Android speech recognition stopped");
        }
    }

    public async Task<string?> ListenUntilSilence(
        SpeechRecognitionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        string? lastText = null;
        await foreach (var result in ContinuousRecognize(options, cancellationToken))
        {
            lastText = result.Text;
            if (result.IsFinal)
                return result.Text;
        }
        return lastText;
    }

    sealed class SpeechListener(ChannelWriter<SpeechRecognitionResult> writer, ILogger logger) : Java.Lang.Object, IRecognitionListener
    {
        public void OnResults(Bundle? results)
        {
            var matches = results?.GetStringArrayList(Android.Speech.SpeechRecognizer.ResultsRecognition);
            var text = matches?.FirstOrDefault();
            if (text != null)
            {
                float? confidence = null;
                var scores = results?.GetFloatArray(Android.Speech.SpeechRecognizer.ConfidenceScores);
                if (scores is { Length: > 0 })
                    confidence = scores[0];

                writer.TryWrite(new SpeechRecognitionResult(text, true, confidence));
            }
            writer.TryComplete();
        }

        public void OnPartialResults(Bundle? partialResults)
        {
            var matches = partialResults?.GetStringArrayList(Android.Speech.SpeechRecognizer.ResultsRecognition);
            var text = matches?.FirstOrDefault();
            if (!string.IsNullOrEmpty(text))
                writer.TryWrite(new SpeechRecognitionResult(text, false));
        }

        public void OnError(SpeechRecognizerError error)
        {
            logger.LogWarning("Speech recognition error: {Error}", error);
            if (error == SpeechRecognizerError.NoMatch || error == SpeechRecognizerError.SpeechTimeout)
            {
                writer.TryComplete();
            }
            else
            {
                writer.TryComplete(new InvalidOperationException($"Speech recognition error: {error}"));
            }
        }

        public void OnReadyForSpeech(Bundle? @params) =>
            logger.LogDebug("Ready for speech");

        public void OnBeginningOfSpeech() =>
            logger.LogDebug("Beginning of speech");

        public void OnEndOfSpeech() =>
            logger.LogDebug("End of speech");

        public void OnRmsChanged(float rmsdB) { }
        public void OnBufferReceived(byte[]? buffer) { }
        public void OnEvent(int eventType, Bundle? @params) { }
    }
}
#endif
