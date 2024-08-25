using System.ComponentModel.DataAnnotations;
using CodeFlows.Workspace.Common.DTOs;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace CodeFlows.Workspace.Util.Workers
{
    public record ForkProjectAnalysis : IRequest<ForkProjectAnalysis.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        [Required]
        public required string RepositoryName { get; set; }

        [Required]
        public required Dictionary<
            string,
            ProjectDetectionTaskOutput
        > DetectionForkJoinResults { get; set; }

        public record Response(
            List<WorkflowTask> Tasks,
            Dictionary<string, RefactorProjectWorkflowInput> WorkflowInputs
        );

        [OriginalName("fork_project_analysis")]
        public class Handler(IMetadataService metadataService)
            : TaskRequestHandler<ForkProjectAnalysis, Response>
        {
            private readonly IMetadataService metadataService = metadataService;
            private const string projectRefactorWorkflowPrefix = "refactor_project_";

            public override async Task<Response> Handle(
                ForkProjectAnalysis request,
                CancellationToken cancellationToken
            )
            {
                var workflows = await metadataService.ListWorkflowsAsync(cancellationToken);

                var refactorProjectWorkflows = workflows
                    .Where(w => w.Name.StartsWith(projectRefactorWorkflowPrefix))
                    .GroupBy(w => w.Name)
                    .ToDictionary(
                        w => GetRefactorProjectTypeFromName(w.Key),
                        w => w.OrderByDescending(w => w.Version).First()
                    );

                var dynamicTasks = new List<WorkflowTask>();
                var dynamicTaskInputs = new Dictionary<string, RefactorProjectWorkflowInput>();
                var workflowIndex = 0;

                foreach (var detectedProject in request.DetectionForkJoinResults.Select(detectedProject => detectedProject.Value))
                {
                    if (
                        !refactorProjectWorkflows.TryGetValue(
                            detectedProject.ProjectType,
                            out var workflowDefinition
                        )
                    )
                    {
                        throw new InvalidOperationException(
                            $"Could not match refactor workflow for project"
                        );
                    }

                    foreach (var projectPath in detectedProject.ProjectFilePaths)
                    {
                        var referenceName = $"{workflowDefinition.Name}_{workflowIndex++}";

                        dynamicTasks.Add(
                            new WorkflowTask
                            {
                                Name = workflowDefinition.Name,
                                TaskReferenceName = referenceName,
                                SubWorkflowParam = new()
                                {
                                    Name = workflowDefinition.Name,
                                    Version = workflowDefinition.Version
                                },
                                Type = "SUB_WORKFLOW"
                            }
                        );

                        dynamicTaskInputs.Add(
                            referenceName,
                            new(request.RepositoryPath, projectPath, request.RepositoryName)
                        );
                    }
                }

                return new Response(dynamicTasks, dynamicTaskInputs);
            }

            private static string GetRefactorProjectTypeFromName(string name) =>
                name.Replace(projectRefactorWorkflowPrefix, "");
        }
    }
}