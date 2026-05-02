namespace Shiny.Speech;

/// <summary>
/// Platform-specific audio playback for synthesized speech.
/// </summary>
public interface IAudioPlayer : IAsyncDisposable
{
    /// <summary>
    /// Play an audio stream (MP3 format). Completes when playback finishes or is cancelled.
    /// </summary>
    Task PlayAsync(Stream audioStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop any current playback.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Whether audio is currently playing.
    /// </summary>
    bool IsPlaying { get; }
}
