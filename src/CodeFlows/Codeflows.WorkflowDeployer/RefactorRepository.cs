using Codeflows.Portal.Application.Workers;
using Codeflows.Portal.Infrastructure.Persistence.Entities;
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
    [WorkflowMetadata(FailureWorkflow = typeof(HandleRefactorFailure))]
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
            public required int RefactorRunId { get; set; }
            public required string RepositoryPath { get; set; }
        }

        public class RefactorRepositoryOutput : WorkflowOutput { }

        public required CloneProject.Handler CloneRepository { get; set; }
        public required ForkProjectDetection.Handler PrepareForkDetectProjects { get; set; }
        public required ForkProjectAnalysis.Handler PrepareForkAnalysis { get; set; }
        public required DynamicForkJoinTaskModel ForkDetectProjects { get; set; }
        public required DynamicForkJoinTaskModel ForkAnalysis { get; set; }
        public required CommitProjectChanges.Handler CommitChanges { get; set; }
        public required CreatePullRequest.Handler CreatePullRequest { get; set; }
        public required Cleanup.Handler Cleanup { get; set; }
        public required UpdateRefactorRun.Handler UpdateStateToCompleted { get; set; }
        public required UpdateRefactorRun.Handler UpdateStateToRunning { get; set; }

        public override void BuildDefinition()
        {
            // TODO: Check if there is already a pull request open from us, and fail immediately if so


            _builder.AddTask(
                wf => wf.UpdateStateToRunning,
                wf => new UpdateRefactorRun()
                {
                    RefactorRunId = wf.Input.RefactorRunId,
                    State = RefactorRunState.Running
                }
            );

            _builder.AddTask(
                wf => wf.CloneRepository,
                wf => new CloneProject()
                {
                    RepositoryUrl = wf.Input.RepositoryUrl,
                    RepositoryPath = wf.Input.RepositoryPath
                }
            );

            _builder.AddTask(
                wf => wf.PrepareForkDetectProjects,
                wf => new ForkProjectDetection
                {
                    RepositoryPath = wf.CloneRepository.Output.RepositoryPath
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
                            (object)"${JOIN_fork_detect_projects.output}",
                    RepositoryName = wf.CloneRepository.Output.RepositoryName,
                    RepositoryPath = wf.CloneRepository.Output.RepositoryPath
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

            _builder.AddTask(
                wf => wf.CommitChanges,
                wf => new CommitProjectChanges()
                {
                    RepositoryPath = wf.CloneRepository.Output.RepositoryPath,
                    CommitMessage = "Apply code refactor suggestions through AI generated code"
                }
            );

            _builder.AddTask(
                wf => wf.CreatePullRequest,
                wf => new CreatePullRequest()
                {
                    RepositoryPath = wf.CloneRepository.Output.RepositoryPath,
                    OriginalOwner = wf.CloneRepository.Output.RepositoryOwner,
                    RepositoryOwner = wf.CloneRepository.Output.RepositoryOwner,
                    RepositoryName = wf.CloneRepository.Output.RepositoryName,
                    BranchName = wf.CloneRepository.Output.BranchName,
                    PullRequestDescription =
                        "# Pull Request: AI-Driven Code Quality Improvements\r\n\r\n## Overview\r\n\r\nThis pull request includes code changes that have been automatically generated by AI agents. The AI agents followed well-known code quality rules and metrics to enhance the overall codebase. These improvements are aimed at making the code cleaner, more maintainable, and adhering to best practices.\r\n",
                    PullRequestTitle = "Refactor using AI",
                    BaseRef = wf.CloneRepository.Output.DefaultBranch,
                    AreThereAnyChanges = wf.CommitChanges.Output.AreThereAnyChanges
                }
            );

            _builder.AddTask(
                wf => wf.Cleanup,
                wf => new Cleanup() { RepositoryPath = wf.CloneRepository.Output.RepositoryPath }
            );

            _builder.AddTask(
                wf => wf.UpdateStateToCompleted,
                wf => new UpdateRefactorRun()
                {
                    RefactorRunId = wf.Input.RefactorRunId,
                    State = RefactorRunState.Success,
                    PullRequestUrl = wf.CreatePullRequest.Output.PullRequestUrl
                }
            );
        }
    }
}
