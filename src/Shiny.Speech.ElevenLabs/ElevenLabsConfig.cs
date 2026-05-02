namespace Shiny.Speech.ElevenLabs;

public record ElevenLabsConfig
{
    public required string ApiKey { get; init; }

    /// <summary>
    /// Default voice ID to use when no voice is specified in options.
    /// Default: "21m00Tcm4TlvDq8ikWAM" (Rachel)
    /// </summary>
    public string DefaultVoiceId { get; init; } = "21m00Tcm4TlvDq8ikWAM";

    /// <summary>
    /// The TTS model to use. Default: "eleven_multilingual_v2"
    /// </summary>
    public string ModelId { get; init; } = "eleven_multilingual_v2";
}
