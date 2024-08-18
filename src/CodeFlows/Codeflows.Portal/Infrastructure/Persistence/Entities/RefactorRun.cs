namespace Codeflows.Portal.Infrastructure.Persistence.Entities
{
    public enum RefactorRunState
    {
        Scheduled,
        Running,
        Success,
        Failure,
        Rejected
    }

    public class RefactorRun
    {
        public int Id { get; set; }
        public string? WorkflowId { get; set; }
        public string? Note { get; set; }
        public required string RepositoryUrl { get; set; }
        public string? PullRequestUrl { get; set; }
        public RefactorRunState State { get; set; }
    }
}
