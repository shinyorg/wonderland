using System.Globalization;

namespace Shiny.Speech;

public record TextToSpeechOptions
{
    /// <summary>
    /// Language/locale for speech. Null uses the device default.
    /// </summary>
    public CultureInfo? Culture { get; init; }

    /// <summary>
    /// The voice to use for speech. Null uses the default voice for the culture.
    /// Use <see cref="ITextToSpeechService.GetVoicesAsync"/> to list available voices.
    /// </summary>
    public VoiceInfo? Voice { get; init; }

    /// <summary>
    /// Speech rate multiplier. 1.0 is normal speed, 0.5 is half speed, 2.0 is double speed.
    /// Clamped to platform-supported ranges.
    /// Default: 1.0
    /// </summary>
    public float SpeechRate { get; init; } = 1.0f;

    /// <summary>
    /// Pitch multiplier. 1.0 is normal pitch, 0.5 is lower, 2.0 is higher.
    /// Clamped to platform-supported ranges.
    /// Default: 1.0
    /// </summary>
    public float Pitch { get; init; } = 1.0f;

    /// <summary>
    /// Volume. 0.0 is silent, 1.0 is maximum.
    /// Default: 1.0
    /// </summary>
    public float Volume { get; init; } = 1.0f;
}
