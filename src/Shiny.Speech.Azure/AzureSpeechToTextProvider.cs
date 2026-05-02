using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Shiny.Speech.Cloud;

namespace Shiny.Speech.Azure;

public class AzureSpeechToTextProvider(
    AzureSpeechConfig config,
    ILogger<AzureSpeechToTextProvider> logger
) : ISpeechToTextProvider
{
    public async IAsyncEnumerable<SpeechRecognitionResult> RecognizeAsync(
        Stream audioStream,
        SpeechRecognitionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new SpeechRecognitionOptions();

        var speechConfig = SpeechConfig.FromSubscription(config.SubscriptionKey, config.Region);
        if (options.Culture != null)
            speechConfig.SpeechRecognitionLanguage = options.Culture.Name;

        var silenceMs = ((int)options.SilenceTimeout.TotalMilliseconds).ToString();
        speechConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, silenceMs);

        using var pushStream = AudioInputStream.CreatePushStream(
            AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1)
        );
        using var audioConfig = AudioConfig.FromStreamInput(pushStream);
        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        var channel = Channel.CreateUnbounded<SpeechRecognitionResult>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        });

        recognizer.Recognizing += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Result.Text))
                channel.Writer.TryWrite(new SpeechRecognitionResult(e.Result.Text, false));
        };

        recognizer.Recognized += (_, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrEmpty(e.Result.Text))
                channel.Writer.TryWrite(new SpeechRecognitionResult(e.Result.Text, true));
        };

        recognizer.SessionStopped += (_, _) => channel.Writer.TryComplete();

        recognizer.Canceled += (_, e) =>
        {
            if (e.Reason == CancellationReason.Error)
            {
                logger.LogError("Azure STT canceled with error: {ErrorCode} {ErrorDetails}", e.ErrorCode, e.ErrorDetails);
                channel.Writer.TryComplete(new InvalidOperationException($"Azure STT error: {e.ErrorDetails}"));
            }
            else
            {
                channel.Writer.TryComplete();
            }
        };

        await recognizer.StartContinuousRecognitionAsync();
        logger.LogDebug("Azure continuous recognition started");

        // Pump audio data to Azure in the background
        _ = Task.Run(async () =>
        {
            try
            {
                var buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    pushStream.Write(buffer, bytesRead);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error pumping audio to Azure");
            }
            finally
            {
                pushStream.Close();
            }
        }, cancellationToken);

        try
        {
            await foreach (var result in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return result;
            }
        }
        finally
        {
            await recognizer.StopContinuousRecognitionAsync();
            logger.LogDebug("Azure continuous recognition stopped");
        }
    }
}
