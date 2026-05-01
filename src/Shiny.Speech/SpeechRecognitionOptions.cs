using System.Globalization;

namespace Shiny.Speech;

public record SpeechRecognitionOptions
{
    /// <summary>
    /// Language/locale for recognition. Null uses the device default.
    /// </summary>
    public CultureInfo? Culture { get; init; }

    /// <summary>
    /// Duration of silence after speech before the recognizer stops and returns the final result.
    /// Default: 2 seconds.
    /// </summary>
    public TimeSpan SilenceTimeout { get; init; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Prefer on-device recognition when available (iOS 13+).
    /// Falls back to server-based recognition if not supported.
    /// </summary>
    public bool PreferOnDevice { get; init; }
}
