using System.Text.Json.Serialization;

namespace ShinyWonderland.Services;

// [Bind] properties on AppSettings persist through Shiny.Json, which is AOT-safe (registered
// contexts only, no reflection fallback). Without this, setting AppSettings.Ordering throws
// "No JsonTypeInfo registered for type RideOrder". [ShinyJsonContext] auto-registers this into
// Shiny.Json via a generated module initializer.
[ShinyJsonContext]
[JsonSerializable(typeof(RideOrder))]
internal partial class SettingsJsonContext : JsonSerializerContext;
