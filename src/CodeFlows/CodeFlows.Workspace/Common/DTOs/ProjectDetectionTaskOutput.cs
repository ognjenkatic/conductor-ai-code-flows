namespace CodeFlows.Workspace.Common.DTOs
{
    public record ProjectDetectionTaskOutput(
        List<string> ProjectFilePaths,
        string ProjectType,
        string ProjectName,
        string RepositoryPath,
        string RepositoryName
    );
}
