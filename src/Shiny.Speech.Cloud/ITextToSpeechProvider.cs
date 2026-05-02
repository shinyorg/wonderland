using System.Globalization;
using Shiny.Speech;

namespace Shiny.Speech.Cloud;

/// <summary>
/// Implement this interface to plug in a cloud text-to-speech provider (Azure, ElevenLabs, etc.).
/// The provider synthesizes text into an audio stream for playback.
/// </summary>
public interface ITextToSpeechProvider
{
    /// <summary>
    /// Gets available voices, optionally filtered by culture.
    /// </summary>
    Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synthesize text into an audio stream (MP3 format).
    /// </summary>
    Task<Stream> SynthesizeAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default);
}
