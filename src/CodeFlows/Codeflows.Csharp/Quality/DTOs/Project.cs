using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class Project
    {
        [JsonPropertyName("key")]
        public required string Key { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("qualifier")]
        public required string Qualifier { get; set; }

        [JsonPropertyName("visibility")]
        public required string Visibility { get; set; }

        [JsonPropertyName("Revision")]
        public string? Revision { get; set; }

        [JsonPropertyName("managed")]
        public required bool Managed { get; set; }
        //[JsonPropertyName("lastAnalysisDate")]
        //public required DateTimeOffset LastAnalysisDate { get; set; }
    }
}
