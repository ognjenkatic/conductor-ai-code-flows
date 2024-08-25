using Codeflows.Portal.Application.Workers;
using Codeflows.Portal.Infrastructure.Persistence.Entities;
using CodeFlows.Workspace.Github.Workers;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;

namespace Codeflows.WorkflowDeployer
{
    [OriginalName("handle_refactor_failure")]
    [Version(1)]
    public class HandleRefactorFailure(
        WorkflowDefinitionBuilder<
            HandleRefactorFailure,
            HandleRefactorFailure.HandleRefactorFailureInput,
            HandleRefactorFailure.HandleRefactorFailureOutput
        > builder
    )
        : Workflow<
            HandleRefactorFailure,
            HandleRefactorFailure.HandleRefactorFailureInput,
            HandleRefactorFailure.HandleRefactorFailureOutput
        >(builder)
    {
        public class HandleRefactorFailureInput : WorkflowInput<HandleRefactorFailureOutput>
        {
            public required string RepositoryPath { get; set; }
            public required int RefactorRunId { get; set; }
        }

        public class HandleRefactorFailureOutput : WorkflowOutput 
        {
            // Implementation or properties can be added here if needed in the future.
        }

        public required Cleanup.Handler Cleanup { get; set; }
        public required UpdateRefactorRun.Handler UpdateStateToFailed { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.Cleanup,
                wf => new Cleanup { RepositoryPath = wf.Input.RepositoryPath }
            );

            _builder.AddTask(
                wf => wf.UpdateStateToFailed,
                wf => new UpdateRefactorRun()
                {
                    RefactorRunId = wf.Input.RefactorRunId,
                    State = RefactorRunState.Failure
                }
            );
        }
    }
}