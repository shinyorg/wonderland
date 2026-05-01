namespace Shiny.Speech;

public record SpeechRecognitionResult(
    string Text,
    bool IsFinal,
    float? Confidence = null
);
