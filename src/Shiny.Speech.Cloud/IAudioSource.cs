namespace Shiny.Speech.Cloud;

/// <summary>
/// Platform-specific audio capture source that provides raw PCM audio data.
/// </summary>
public interface IAudioSource : IAsyncDisposable
{
    /// <summary>
    /// Start capturing audio from the microphone.
    /// Returns a stream of raw PCM audio data (16kHz, 16-bit, mono).
    /// </summary>
    Task<Stream> StartCaptureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop capturing audio.
    /// </summary>
    Task StopCaptureAsync();
}
