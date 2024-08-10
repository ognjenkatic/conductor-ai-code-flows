using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class Paging
    {
        [JsonPropertyName("pageIndex")]
        public required int PageIndex { get; set; }

        [JsonPropertyName("pageSize")]
        public required int PageSize { get; set; }

        [JsonPropertyName("total")]
        public required int Total { get; set; }
    }
}
