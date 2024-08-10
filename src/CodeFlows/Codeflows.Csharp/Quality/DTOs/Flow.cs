using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class Flow
    {
        [JsonPropertyName("locations")]
        public List<Location> Locations { get; set; } = [];
    }
}
