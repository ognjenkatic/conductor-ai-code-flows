using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class TextRange
    {
        [JsonPropertyName("startLine")]
        public required int StartLine { get; set; }

        [JsonPropertyName("endLine")]
        public required int EndLine { get; set; }

        [JsonPropertyName("startOffset")]
        public required int StartOffset { get; set; }

        [JsonPropertyName("endOffset")]
        public required int EndOffset { get; set; }
    }
}
