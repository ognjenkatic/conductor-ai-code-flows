using Codeflows.Csharp.Quality.Workers;
using CodeFlows.GenAi.OpenAi.Workers;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;

namespace Codeflows.WorkflowDeployer
{
    [OriginalName("refactor_project_csharp")]
    [Version(1)]
    public class CsharpRefactorProject(
        WorkflowDefinitionBuilder<
            CsharpRefactorProject,
            CsharpRefactorProject.CsharpRefactorProjectInput,
            CsharpRefactorProject.CsharpRefactorProjectOutput
        > builder
    )
        : Workflow<
            CsharpRefactorProject,
            CsharpRefactorProject.CsharpRefactorProjectInput,
            CsharpRefactorProject.CsharpRefactorProjectOutput
        >(builder)
    {
        public class CsharpRefactorProjectInput : WorkflowInput<CsharpRefactorProjectOutput>
        {
            public required string RepositoryPath { get; set; }
            public required string RepositoryName { get; set; }
            public required string ProjectPath { get; set; }
        }

        public class CsharpRefactorProjectOutput : WorkflowOutput { }

        public required InitMetricsProject.Handler InitMetricsProject { get; set; }
        public required AnalyseCode.Handler AnalyseCode { get; set; }
        public required GetCodeMetrics.Handler GetMetrics { get; set; }
        public required RefactorProject.Handler RefactorProject { get; set; }
        public required TestBuild.Handler TestBuild { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.InitMetricsProject,
                wf => new InitMetricsProject { ProjectId = wf.Input.RepositoryName }
            );

            _builder.AddTask(
                wf => wf.AnalyseCode,
                wf => new AnalyseCode
                {
                    ProjectId = wf.Input.RepositoryName,
                    ProjectFilePath = wf.Input.ProjectPath
                }
            );

            _builder.AddTask(
                wf => wf.GetMetrics,
                wf => new GetCodeMetrics { ProjectId = wf.Input.RepositoryName, }
            );

            _builder.AddTask(
                wf => wf.RefactorProject,
                wf => new RefactorProject()
                {
                    ProjectFilePath = wf.Input.ProjectPath,
                    MaximumParallelPrompts = 10,
                    IssuesByFile =
                        (Dictionary<string, List<CodeFlows.GenAi.OpenAi.DTOs.CodeFlowIssue>>)
                            (object)wf.GetMetrics.Output.IssuesByFile
                }
            );

            // TODO: Reintroduce this once we actually have time
            // Without this we might make pull requests that make code unbuildable!
            //_builder.AddTask(
            //    wf => wf.TestBuild,
            //    wf => new TestBuild { ProjectFilePath = wf.Input.RepositoryPath }
            //);
        }
    }
}
