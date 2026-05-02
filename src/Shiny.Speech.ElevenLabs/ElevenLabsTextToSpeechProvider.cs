using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Shiny.Speech.Cloud;

namespace Shiny.Speech.ElevenLabs;

public class ElevenLabsTextToSpeechProvider(
    ElevenLabsConfig config,
    ILogger<ElevenLabsTextToSpeechProvider> logger
) : ITextToSpeechProvider
{
    readonly HttpClient httpClient = CreateHttpClient(config);

    static HttpClient CreateHttpClient(ElevenLabsConfig config)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.elevenlabs.io/")
        };
        client.DefaultRequestHeaders.Add("xi-api-key", config.ApiKey);
        return client;
    }

    public async Task<IReadOnlyList<VoiceInfo>> GetVoicesAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("v1/voices", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VoicesResponse>(cancellationToken);
        if (result?.Voices == null)
            return [];

        var voices = new List<VoiceInfo>();
        foreach (var v in result.Voices)
        {
            if (v.VoiceId == null || v.Name == null)
                continue;

            var voiceCulture = CultureInfo.InvariantCulture;
            if (v.Labels?.TryGetValue("language", out var lang) == true && lang != null)
            {
                try { voiceCulture = new CultureInfo(lang); }
                catch { /* use invariant */ }
            }

            if (culture == null || voiceCulture.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName)
                voices.Add(new VoiceInfo(v.VoiceId, v.Name, voiceCulture));
        }

        logger.LogDebug("ElevenLabs returned {Count} voices", voices.Count);
        return voices;
    }

    public async Task<Stream> SynthesizeAsync(string text, TextToSpeechOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new TextToSpeechOptions();
        var voiceId = options.Voice?.Id ?? config.DefaultVoiceId;

        var requestBody = new TtsRequest
        {
            Text = text,
            ModelId = config.ModelId,
            VoiceSettings = new VoiceSettings
            {
                Stability = 0.5f,
                SimilarityBoost = 0.75f
            }
        };

        var response = await httpClient.PostAsJsonAsync($"v1/text-to-speech/{voiceId}", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var audioStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        logger.LogDebug("ElevenLabs TTS synthesized {Bytes} bytes", ms.Length);
        return ms;
    }

    sealed record VoicesResponse
    {
        [JsonPropertyName("voices")]
        public List<VoiceEntry>? Voices { get; init; }
    }

    sealed record VoiceEntry
    {
        [JsonPropertyName("voice_id")]
        public string? VoiceId { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("labels")]
        public Dictionary<string, string?>? Labels { get; init; }
    }

    sealed record TtsRequest
    {
        [JsonPropertyName("text")]
        public required string Text { get; init; }

        [JsonPropertyName("model_id")]
        public required string ModelId { get; init; }

        [JsonPropertyName("voice_settings")]
        public required VoiceSettings VoiceSettings { get; init; }
    }

    sealed record VoiceSettings
    {
        [JsonPropertyName("stability")]
        public float Stability { get; init; }

        [JsonPropertyName("similarity_boost")]
        public float SimilarityBoost { get; init; }
    }
}
