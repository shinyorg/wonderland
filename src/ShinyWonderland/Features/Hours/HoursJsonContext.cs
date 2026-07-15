using System.Text.Json.Serialization;

namespace ShinyWonderland.Features.Hours;

// The source-generated cache converters for ParkHours/TimeRange delegate their DateOnly and
// TimeOnly members to JsonSerializer.Serialize(..., options), which needs a JsonTypeInfo for
// those types. Shiny.Json's options are AOT-safe (registered contexts only, no reflection
// fallback), so without this context DateOnly/TimeOnly resolve to nothing and the persistent
// cache serialize throws — which is why "Hours of Operation" failed to load. [ShinyJsonContext]
// auto-registers this into Shiny.Json via a generated module initializer.
[ShinyJsonContext]
[JsonSerializable(typeof(DateOnly))]
[JsonSerializable(typeof(TimeOnly))]
internal partial class HoursJsonContext : JsonSerializerContext;
