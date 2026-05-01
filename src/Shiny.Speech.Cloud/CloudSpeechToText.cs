using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Shiny.Speech;

namespace Shiny.Speech.Cloud;

/// <summary>
/// ISpeechToText implementation that captures audio from the platform microphone
/// and delegates recognition to a pluggable ISpeechToTextProvider (Azure, Google, etc.).
/// </summary>
public class CloudSpeechToText(
    ISpeechToTextProvider provider,
    IAudioSource audioSource,
    ILogger<CloudSpeechToText> logger
) : ISpeechToText
{
    public bool IsSupported => true;

    public Task<AccessState> RequestAccess()
        // Audio permission is handled by the IAudioSource implementation
        => Task.FromResult(AccessState.Available);

    public async IAsyncEnumerable<SpeechRecognitionResult> ContinuousRecognize(
        SpeechRecognitionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new SpeechRecognitionOptions();

        await using var source = audioSource;
        var audioStream = await source.StartCaptureAsync(cancellationToken);
        logger.LogDebug("Audio capture started for cloud speech recognition");

        try
        {
            await foreach (var result in provider.RecognizeAsync(audioStream, options, cancellationToken))
            {
                yield return result;
                if (result.IsFinal)
                    yield break;
            }
        }
        finally
        {
            await source.StopCaptureAsync();
            logger.LogDebug("Audio capture stopped");
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
