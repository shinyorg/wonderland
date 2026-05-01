#if APPLE
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using AVFoundation;
using Foundation;
using Microsoft.Extensions.Logging;
using Speech;

namespace Shiny.Speech;

public class SpeechToTextImpl(ILogger<SpeechToTextImpl> logger) : ISpeechToText
{
    public bool IsSupported =>
        SFSpeechRecognizer.AuthorizationStatus != SFSpeechRecognizerAuthorizationStatus.Restricted;

    public Task<AccessState> RequestAccess()
    {
        var tcs = new TaskCompletionSource<AccessState>();

        SFSpeechRecognizer.RequestAuthorization(status =>
        {
            switch (status)
            {
                case SFSpeechRecognizerAuthorizationStatus.Authorized:
                    // Also need microphone permission
                    var audioSession = AVAudioSession.SharedInstance();
                    audioSession.RequestRecordPermission(granted =>
                    {
                        tcs.TrySetResult(granted ? AccessState.Available : AccessState.Denied);
                    });
                    break;

                case SFSpeechRecognizerAuthorizationStatus.Denied:
                    tcs.TrySetResult(AccessState.Denied);
                    break;

                case SFSpeechRecognizerAuthorizationStatus.Restricted:
                    tcs.TrySetResult(AccessState.Restricted);
                    break;

                default:
                    tcs.TrySetResult(AccessState.Unknown);
                    break;
            }
        });

        return tcs.Task;
    }

    public async IAsyncEnumerable<SpeechRecognitionResult> ContinuousRecognize(
        SpeechRecognitionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new SpeechRecognitionOptions();

        var locale = options.Culture != null
            ? new NSLocale(options.Culture.Name)
            : NSLocale.CurrentLocale;

        var speechRecognizer = new SFSpeechRecognizer(locale);
        if (!speechRecognizer.Available)
            throw new InvalidOperationException("Speech recognizer is not available for the requested locale.");

        var audioEngine = new AVAudioEngine();
        var request = new SFSpeechAudioBufferRecognitionRequest
        {
            ShouldReportPartialResults = true,
            TaskHint = SFSpeechRecognitionTaskHint.Dictation
        };

        if (options.PreferOnDevice && speechRecognizer.SupportsOnDeviceRecognition)
            request.RequiresOnDeviceRecognition = true;

        var channel = Channel.CreateUnbounded<SpeechRecognitionResult>(new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = true
        });

        CancellationTokenSource? silenceTimer = null;
        var silenceTimeout = options.SilenceTimeout;

        void ResetSilenceTimer()
        {
            silenceTimer?.Cancel();
            silenceTimer?.Dispose();
            silenceTimer = new CancellationTokenSource();
            var token = silenceTimer.Token;
            _ = Task.Delay(silenceTimeout, token).ContinueWith(_ =>
            {
                logger.LogDebug("Silence timeout reached, ending audio");
                request.EndAudio();
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        SFSpeechRecognitionTask? recognitionTask = null;

        try
        {
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Record, AVAudioSessionCategoryOptions.DefaultToSpeaker, out var categoryError);
            if (categoryError != null)
                throw new InvalidOperationException($"Failed to set audio session category: {categoryError.LocalizedDescription}");

            audioSession.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out var activeError);
            if (activeError != null)
                throw new InvalidOperationException($"Failed to activate audio session: {activeError.LocalizedDescription}");

            var inputNode = audioEngine.InputNode;
            var recordingFormat = inputNode.GetBusOutputFormat(0);

            recognitionTask = speechRecognizer.GetRecognitionTask(request, (result, error) =>
            {
                if (error != null)
                {
                    logger.LogError("Speech recognition error: {Error}", error.LocalizedDescription);
                    channel.Writer.TryComplete(new InvalidOperationException(error.LocalizedDescription));
                    return;
                }

                if (result == null)
                    return;

                var text = result.BestTranscription.FormattedString;
                var isFinal = result.Final;

                float? confidence = null;
                var segments = result.BestTranscription.Segments;
                if (segments.Length > 0)
                    confidence = (float)segments[^1].Confidence;

                var speechResult = new SpeechRecognitionResult(text, isFinal, confidence);
                channel.Writer.TryWrite(speechResult);

                if (isFinal)
                {
                    channel.Writer.TryComplete();
                }
                else
                {
                    ResetSilenceTimer();
                }
            });

            inputNode.InstallTapOnBus(0, 1024, recordingFormat, (buffer, when) =>
            {
                request.Append(buffer);
            });

            audioEngine.Prepare();
            audioEngine.StartAndReturnError(out var engineError);
            if (engineError != null)
                throw new InvalidOperationException($"Failed to start audio engine: {engineError.LocalizedDescription}");

            logger.LogDebug("Speech recognition started");

            // Start the silence timer for initial silence detection
            ResetSilenceTimer();

            using var reg = cancellationToken.Register(() =>
            {
                request.EndAudio();
                channel.Writer.TryComplete();
            });

            await foreach (var result in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                yield return result;
            }
        }
        finally
        {
            silenceTimer?.Cancel();
            silenceTimer?.Dispose();

            if (audioEngine.Running)
            {
                audioEngine.Stop();
                audioEngine.InputNode.RemoveTapOnBus(0);
            }

            recognitionTask?.Cancel();

            var session = AVAudioSession.SharedInstance();
            session.SetActive(false, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out _);

            logger.LogDebug("Speech recognition stopped");
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
}
#endif
