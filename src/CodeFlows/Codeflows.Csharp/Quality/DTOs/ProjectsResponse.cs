using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class ProjectsResponse
    {
        [JsonPropertyName("paging")]
        public required Paging Paging { get; set; }

        [JsonPropertyName("components")]
        public List<Project> Components { get; set; } = [];
    }
}
