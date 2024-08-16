namespace Codeflows.Portal.DTOs
{
    public class RefactorRunDetails
    {
        public int Id { get; set; }
        public required string WorkflowId { get; set; }
        public required string RepositoryUrl { get; set; }
        public required string State { get; set; }
    }
}
