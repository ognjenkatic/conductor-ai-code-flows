using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class IssuesResponse
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("p")]
        public int? P { get; set; }

        [JsonPropertyName("ps")]
        public int? Ps { get; set; }

        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }

        [JsonPropertyName("effortTotal")]
        public int? EffortTotal { get; set; }

        [JsonPropertyName("issues")]
        public List<Issue> Issues { get; set; } = [];

        [JsonPropertyName("components")]
        public List<Component> Components { get; set; } = [];

        [JsonPropertyName("facets")]
        public List<object> Facets { get; set; } = [];
    }
}
