using Codeflows.Csharp.Quality.Workers;
using CodeFlows.GenAi.OpenAi.Workers;
using CodeFlows.Workspace.Common.DTOs;
using CodeFlows.Workspace.Github.Workers;
using CodeFlows.Workspace.Util.Workers;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Model;

namespace Codeflows.WorkflowDeployer
{
    [OriginalName("refactor_repository")]
    [Version(1)]
    public class RefactorRepository(
        WorkflowDefinitionBuilder<
            RefactorRepository,
            RefactorRepository.RefactorRepositoryInput,
            RefactorRepository.RefactorRepositoryOutput
        > builder
    )
        : Workflow<
            RefactorRepository,
            RefactorRepository.RefactorRepositoryInput,
            RefactorRepository.RefactorRepositoryOutput
        >(builder)
    {
        public class RefactorRepositoryInput : WorkflowInput<RefactorRepositoryOutput>
        {
            public required string RepositoryUrl { get; set; }
            public required string RepositoryName { get; set; }
        }

        public class RefactorRepositoryOutput : WorkflowOutput { }

        public required CloneProject.Handler CloneRepository { get; set; }
        public required ForkProjectDetection.Handler PrepareForkDetectProjects { get; set; }
        public required ForkProjectAnalysis.Handler PrepareForkAnalysis { get; set; }
        public required DynamicForkJoinTaskModel ForkDetectProjects { get; set; }
        public required DynamicForkJoinTaskModel ForkAnalysis { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.CloneRepository,
                wf => new CloneProject() { RepositoryUrl = wf.Input.RepositoryUrl }
            );

            _builder.AddTask(
                wf => wf.PrepareForkDetectProjects,
                wf => new ForkProjectDetection
                {
                    RepositoryPath = wf.CloneRepository.Output.RepositoryPath,
                    RepositoryName = wf.Input.RepositoryName
                }
            );

            _builder.AddTask(
                wf => wf.ForkDetectProjects,
                wf =>
                    new()
                    {
                        DynamicTasks = wf.PrepareForkDetectProjects.Output.Tasks,
                        DynamicTasksI = wf.PrepareForkDetectProjects.Output.TaskInputs
                    }
            );

            _builder.AddTask(
                wf => wf.PrepareForkAnalysis,
                wf => new ForkProjectAnalysis
                {
                    DetectionForkJoinResults =
                        (Dictionary<string, ProjectDetectionTaskOutput>)
                            (object)"${JOIN_fork_detect_projects.output}"
                }
            );

            _builder.AddTask(
                wf => wf.ForkAnalysis,
                wf =>
                    new()
                    {
                        DynamicTasks = wf.PrepareForkAnalysis.Output.Tasks,
                        DynamicTasksI = wf.PrepareForkAnalysis.Output.WorkflowInputs
                    }
            );
        }
    }
}
