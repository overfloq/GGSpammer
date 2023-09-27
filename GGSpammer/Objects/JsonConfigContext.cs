using System.Text.Json.Serialization;

namespace GGSpammer.Objects;
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(JsonConfig))]
internal partial class JsonConfigContext : JsonSerializerContext
{
}