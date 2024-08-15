namespace CodeFlows.Workspace.Common.DTOs
{
    public record RefactorProjectWorkflowInput(
        string RepositoryPath,
        string ProjectPath,
        string RepositoryName
    );
}
