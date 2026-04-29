using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace ShinyWonderland.Features.AI.Handlers;

[Description("Gets current weather conditions and forecast for a given date at the park. Returns temperature, humidity, wind, precipitation chance, UV index, and a natural language summary.")]
public record GetWeather(
    [Description("The date to retrieve weather for. Use the current date/time for today's weather, or a future date for the forecast.")]
    DateTimeOffset When
) : IRequest<WeatherResult>;

public record WeatherResult(
    string LocationName,
    DateTimeOffset Date,
    double TemperatureCelsius,
    double FeelsLikeCelsius,
    double HighCelsius,
    double LowCelsius,
    int HumidityPercent,
    double WindSpeedKmh,
    string Condition,
    int PrecipitationProbabilityPercent,
    double PrecipitationMm,
    int UvIndex,
    string Summary
);

[MediatorSingleton]
public partial class GetWeatherHandler(
    IHttpClientFactory httpClientFactory,
    IOptions<ParkOptions> parkOptions,
    TimeProvider timeProvider,
    ILogger<GetWeatherHandler> logger
) : IRequestHandler<GetWeather, WeatherResult>
{
    const string ForecastBaseUrl = "https://api.open-meteo.com/v1/forecast";

    [Cache(AbsoluteExpirationSeconds = 60 * 60)]
    public async Task<WeatherResult> Handle(
        GetWeather request,
        IMediatorContext context,
        CancellationToken ct)
    {
        var park = parkOptions.Value;
        var forecast = await this.FetchForecastAsync(park.Latitude, park.Longitude, ct);

        var targetDate = request.When.Date;
        var isToday = targetDate == timeProvider.GetLocalNow().Date;

        var current = forecast.GetProperty("current");
        var daily = forecast.GetProperty("daily");
        var dates = daily.GetProperty("time");

        var dayIndex = FindDayIndex(dates, targetDate);
        if (dayIndex < 0)
        {
            return new WeatherResult(
                LocationName: park.Name,
                Date: request.When,
                TemperatureCelsius: 0, FeelsLikeCelsius: 0,
                HighCelsius: 0, LowCelsius: 0,
                HumidityPercent: 0, WindSpeedKmh: 0,
                Condition: "Unknown",
                PrecipitationProbabilityPercent: 0,
                PrecipitationMm: 0, UvIndex: 0,
                Summary: $"Weather forecast is not available for {targetDate:MMMM dd, yyyy}. Only the next 16 days are supported."
            );
        }

        double temp, feelsLike;
        int humidity;
        double windSpeed;
        int weatherCode;

        if (isToday)
        {
            temp = current.GetProperty("temperature_2m").GetDouble();
            feelsLike = current.GetProperty("apparent_temperature").GetDouble();
            humidity = current.GetProperty("relative_humidity_2m").GetInt32();
            windSpeed = current.GetProperty("wind_speed_10m").GetDouble();
            weatherCode = current.GetProperty("weather_code").GetInt32();
        }
        else
        {
            var high = GetDailyDouble(daily, "temperature_2m_max", dayIndex);
            var low = GetDailyDouble(daily, "temperature_2m_min", dayIndex);
            temp = (high + low) / 2.0;
            feelsLike = temp;
            humidity = 0;
            windSpeed = 0;
            weatherCode = GetDailyInt(daily, "weather_code", dayIndex);
        }

        var high_ = GetDailyDouble(daily, "temperature_2m_max", dayIndex);
        var low_ = GetDailyDouble(daily, "temperature_2m_min", dayIndex);
        var precipProb = GetDailyInt(daily, "precipitation_probability_max", dayIndex);
        var precipMm = GetDailyDouble(daily, "precipitation_sum", dayIndex);
        var uvIndex = (int)GetDailyDouble(daily, "uv_index_max", dayIndex);
        var condition = WmoCodeToCondition(weatherCode);

        var summary = BuildSummary(
            condition, temp, high_, low_,
            precipProb, windSpeed, uvIndex, isToday);

        return new WeatherResult(
            LocationName: park.Name,
            Date: request.When,
            TemperatureCelsius: Math.Round(temp, 1),
            FeelsLikeCelsius: Math.Round(feelsLike, 1),
            HighCelsius: Math.Round(high_, 1),
            LowCelsius: Math.Round(low_, 1),
            HumidityPercent: humidity,
            WindSpeedKmh: Math.Round(windSpeed, 1),
            Condition: condition,
            PrecipitationProbabilityPercent: precipProb,
            PrecipitationMm: Math.Round(precipMm, 1),
            UvIndex: uvIndex,
            Summary: summary
        );
    }

    async Task<JsonElement> FetchForecastAsync(double lat, double lon, CancellationToken ct)
    {
        var http = httpClientFactory.CreateClient();
        var url = string.Format(
            CultureInfo.InvariantCulture,
            "{0}?latitude={1:F4}&longitude={2:F4}" +
            "&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,weather_code,wind_speed_10m" +
            "&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,precipitation_probability_max,uv_index_max,weather_code" +
            "&timezone=auto&forecast_days=16",
            ForecastBaseUrl, lat, lon);

        logger.LogInformation("Fetching forecast for {Lat},{Lon}", lat, lon);
        return await http.GetFromJsonAsync<JsonElement>(url, ct);
    }

    static int FindDayIndex(JsonElement dates, DateTimeOffset targetDate)
    {
        var target = targetDate.ToString("yyyy-MM-dd");
        for (var i = 0; i < dates.GetArrayLength(); i++)
        {
            if (dates[i].GetString() == target)
                return i;
        }
        return -1;
    }

    static double GetDailyDouble(JsonElement daily, string property, int index)
    {
        var arr = daily.GetProperty(property);
        return index < arr.GetArrayLength() ? arr[index].GetDouble() : 0;
    }

    static int GetDailyInt(JsonElement daily, string property, int index)
    {
        var arr = daily.GetProperty(property);
        return index < arr.GetArrayLength() ? arr[index].GetInt32() : 0;
    }

    static string WmoCodeToCondition(int code) => code switch
    {
        0 => "Clear sky",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Foggy",
        51 or 53 or 55 => "Drizzle",
        56 or 57 => "Freezing drizzle",
        61 or 63 or 65 => "Rain",
        66 or 67 => "Freezing rain",
        71 or 73 or 75 => "Snow",
        77 => "Snow grains",
        80 or 81 or 82 => "Rain showers",
        85 or 86 => "Snow showers",
        95 => "Thunderstorm",
        96 or 99 => "Thunderstorm with hail",
        _ => "Unknown"
    };

    static string BuildSummary(
        string condition, double temp, double high, double low,
        int precipProb, double windSpeed, int uvIndex, bool isToday)
    {
        var timeFrame = isToday ? "Currently" : "Expected";
        var parts = new List<string>
        {
            $"{timeFrame} {Math.Round(temp, 0)}\u00b0C with {condition.ToLowerInvariant()}.",
            $"High of {Math.Round(high, 0)}\u00b0C, low of {Math.Round(low, 0)}\u00b0C."
        };

        if (precipProb > 10)
            parts.Add($"{precipProb}% chance of precipitation.");

        if (isToday && windSpeed > 20)
            parts.Add($"Windy at {Math.Round(windSpeed, 0)} km/h.");

        if (uvIndex >= 6)
            parts.Add($"High UV index ({uvIndex}) \u2014 sun protection recommended.");

        return string.Join(" ", parts);
    }
}
