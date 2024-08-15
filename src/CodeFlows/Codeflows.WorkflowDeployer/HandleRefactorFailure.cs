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
        }

        public class HandleRefactorFailureOutput : WorkflowOutput { }

        public required Cleanup.Handler Cleanup { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.Cleanup,
                wf => new Cleanup { RepositoryPath = wf.Input.RepositoryPath }
            );
        }
    }
}
