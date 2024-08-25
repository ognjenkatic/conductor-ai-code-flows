using System;

namespace Codeflows.Portal.DTOs
{
    public class RefactorRunDto
    {
        public int Id { get; set; }
        public string? WorkflowId { get; set; }
        public required string RepositoryUrl { get; set; }
        public required string State { get; set; }
        public string? Note { get; set; }
        public string? PullRequestUrl { get; set; }
    }
}