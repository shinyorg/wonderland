using Shiny.Speech;

namespace Shiny.Speech.Cloud;

/// <summary>
/// Implement this interface to plug in a cloud speech-to-text provider (Azure, Google, AWS, etc.).
/// The provider receives raw PCM audio data and yields recognition results.
/// </summary>
public interface ISpeechToTextProvider
{
    /// <summary>
    /// Start a recognition session. The implementation should read audio from the stream
    /// and yield results as they become available.
    /// </summary>
    /// <param name="audioStream">Raw PCM audio stream (16kHz, 16-bit, mono)</param>
    /// <param name="options">Recognition options including culture and silence timeout</param>
    /// <param name="cancellationToken">Cancellation token to stop recognition</param>
    IAsyncEnumerable<SpeechRecognitionResult> RecognizeAsync(
        Stream audioStream,
        SpeechRecognitionOptions? options = null,
        CancellationToken cancellationToken = default
    );
}
