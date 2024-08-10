using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class Component
    {
        [JsonPropertyName("key")]
        public required string Key { get; set; }

        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        [JsonPropertyName("qualifier")]
        public string? Qualifier { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("longName")]
        public string? LongName { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }
    }
}
