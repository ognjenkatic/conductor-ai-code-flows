using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class Location
    {
        [JsonPropertyName("component")]
        public required string Component { get; set; }

        [JsonPropertyName("textRange")]
        public TextRange? TextRange { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("msgFormattings")]
        public List<object> MsgFormattings { get; set; } = [];
    }
}
