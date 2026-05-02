using System.Globalization;

namespace Shiny.Speech;

public interface ITextToSpeechService
{
    /// <summary>
    /// Whether text-to-speech is available on this device
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets the available voices for the given culture, or all voices if no culture is specified
    /// </summary>
    Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Speaks the given text. Completes when speech is finished or cancelled.
    /// If called while already speaking, the current speech is stopped first.
    /// </summary>
    Task SpeakAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops any speech that is currently in progress
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Whether the synthesizer is currently speaking
    /// </summary>
    bool IsSpeaking { get; }
}
