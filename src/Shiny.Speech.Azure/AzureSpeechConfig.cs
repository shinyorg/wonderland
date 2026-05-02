namespace Shiny.Speech.Azure;

public record AzureSpeechConfig
{
    public required string SubscriptionKey { get; init; }
    public required string Region { get; init; }
}
