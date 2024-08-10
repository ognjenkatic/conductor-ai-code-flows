using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class Impact
    {
        [JsonPropertyName("softwareQuality")]
        public required string SoftwareQuality { get; set; }

        [JsonPropertyName("severity")]
        public required string Severity { get; set; }
    }
}
