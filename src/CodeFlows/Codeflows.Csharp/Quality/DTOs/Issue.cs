using System.Text.Json.Serialization;

namespace Codeflows.Csharp.Quality.DTOs
{
    public class Issue
    {
        [JsonPropertyName("key")]
        public required string Key { get; set; }

        [JsonPropertyName("rule")]
        public required string Rule { get; set; }

        [JsonPropertyName("severity")]
        public required string Severity { get; set; }

        [JsonPropertyName("component")]
        public required string Component { get; set; }

        [JsonPropertyName("project")]
        public required string Project { get; set; }

        [JsonPropertyName("line")]
        public int? Line { get; set; }

        [JsonPropertyName("hash")]
        public required string Hash { get; set; }

        [JsonPropertyName("textRange")]
        public required TextRange TextRange { get; set; }

        [JsonPropertyName("flows")]
        public List<Flow> Flows { get; set; } = [];

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("effort")]
        public required string Effort { get; set; }

        [JsonPropertyName("debt")]
        public required string Debt { get; set; }

        [JsonPropertyName("author")]
        public required string Author { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];

        //[JsonPropertyName("creationDate")]
        //public DateTimeOffset CreationDate { get; set; }

        //[JsonPropertyName("updateDate")]
        //public DateTimeOffset UpdateDate { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("scope")]
        public required string Scope { get; set; }

        [JsonPropertyName("quickFixAvailable")]
        public bool? QuickFixAvailable { get; set; }

        [JsonPropertyName("messageFormattings")]
        public List<object> MessageFormattings { get; set; } = [];

        [JsonPropertyName("codeVariants")]
        public List<object> CodeVariants { get; set; } = [];

        [JsonPropertyName("cleanCodeAttribute")]
        public required string CleanCodeAttribute { get; set; }

        [JsonPropertyName("cleanCodeAttributeCategory")]
        public required string CleanCodeAttributeCategory { get; set; }

        [JsonPropertyName("impacts")]
        public List<Impact> Impacts { get; set; } = [];

        [JsonPropertyName("issueStatus")]
        public required string IssueStatus { get; set; }

        [JsonPropertyName("prioritizedRule")]
        public bool? PrioritizedRule { get; set; }

        [JsonPropertyName("externalRuleEngine")]
        public string? ExternalRuleEngine { get; set; }
    }
}
