using System.Globalization;

namespace Shiny.Speech;

public record VoiceInfo(
    string Id,
    string Name,
    CultureInfo Culture
);
