#if WINDOWS
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;

namespace Shiny.Speech;

public class SpeechToTextImpl(ILogger<SpeechToTextImpl> logger) : ISpeechToText
{
    public bool IsSupported => true;

    public async Task<AccessState> RequestAccess()
    {
        try
        {
            using var recognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();
            await recognizer.CompileConstraintsAsync();
            return AccessState.Available;
        }
        catch (UnauthorizedAccessException)
        {
            return AccessState.Denied;
        }
        catch (Exception)
        {
            return AccessState.NotSupported;
        }
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

        Windows.Media.SpeechRecognition.SpeechRecognizer? recognizer = null;

        try
        {
            recognizer = options.Culture != null
                ? new Windows.Media.SpeechRecognition.SpeechRecognizer(new Language(options.Culture.Name))
                : new Windows.Media.SpeechRecognition.SpeechRecognizer();

            recognizer.Timeouts.EndSilenceTimeout = options.SilenceTimeout;
            recognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(0); // disable babble timeout

            var compilationResult = await recognizer.CompileConstraintsAsync();
            if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                throw new InvalidOperationException($"Speech recognizer compilation failed: {compilationResult.Status}");

            recognizer.ContinuousRecognitionSession.ResultGenerated += (_, args) =>
            {
                if (args.Result.Status == SpeechRecognitionResultStatus.Success)
                {
                    var confidence = (float)args.Result.RawConfidence;
                    channel.Writer.TryWrite(new SpeechRecognitionResult(
                        args.Result.Text,
                        true,
                        confidence
                    ));
                }
            };

            recognizer.HypothesisGenerated += (_, args) =>
            {
                channel.Writer.TryWrite(new SpeechRecognitionResult(
                    args.Hypothesis.Text,
                    false
                ));
            };

            recognizer.ContinuousRecognitionSession.Completed += (_, args) =>
            {
                logger.LogDebug("Windows continuous recognition completed: {Status}", args.Status);
                channel.Writer.TryComplete();
            };

            await recognizer.ContinuousRecognitionSession.StartAsync();
            logger.LogDebug("Windows speech recognition started");

            using var reg = cancellationToken.Register(async () =>
            {
                try
                {
                    await recognizer.ContinuousRecognitionSession.StopAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error stopping speech recognition on cancellation");
                    channel.Writer.TryComplete();
                }
            });

            await foreach (var result in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                yield return result;
            }
        }
        finally
        {
            recognizer?.Dispose();
            logger.LogDebug("Windows speech recognition stopped");
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
