using System.Runtime.CompilerServices;

namespace Shiny.Speech;

public interface ISpeechToText
{
    /// <summary>
    /// Whether speech recognition is available on this device
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Request microphone and speech recognition permissions
    /// </summary>
    Task<AccessState> RequestAccess();

    /// <summary>
    /// Stream speech recognition results continuously.
    /// Yields partial results as they arrive, then a final result (IsFinal=true) when silence is detected.
    /// The stream completes after the final result or when the CancellationToken is cancelled.
    /// </summary>
    IAsyncEnumerable<SpeechRecognitionResult> ContinuousRecognize(
        SpeechRecognitionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Listen until silence is detected, then return the final recognized text.
    /// Returns null if cancelled before any speech was detected.
    /// </summary>
    Task<string?> ListenUntilSilence(
        SpeechRecognitionOptions? options = null,
        CancellationToken cancellationToken = default
    );
}
