using System.ComponentModel.DataAnnotations;
using CodeFlows.Workspace.Common.DTOs;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace CodeFlows.Workspace.Util.Workers
{
    public record ForkProjectDetection : IRequest<ForkProjectDetection.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        public record Response(
            List<WorkflowTask> Tasks,
            Dictionary<string, ProjectDetectionTaskInput> TaskInputs
        );

        [OriginalName("fork_project_detection")]
        public class Handler(IMetadataService metadataService)
            : TaskRequestHandler<ForkProjectDetection, Response>
        {
            private readonly IMetadataService metadataService = metadataService;
            private const string projectDetectionTaskPrefix = "detect_projects_";

            public override async Task<Response> Handle(
                ForkProjectDetection request,
                CancellationToken cancellationToken
            )
            {
                var tasks = await metadataService.ListTasksAsync(cancellationToken);

                var detectionTasks = tasks.Where(t =>
                    t.Name.StartsWith(projectDetectionTaskPrefix)
                );

                var dynamicTasks = new List<WorkflowTask>();
                var dynamicTaskInputs = new Dictionary<string, ProjectDetectionTaskInput>();

                foreach (var task in detectionTasks)
                {
                    dynamicTasks.Add(
                        new WorkflowTask { Name = task.Name, TaskReferenceName = task.Name }
                    );

                    dynamicTaskInputs.Add(task.Name, new(request.RepositoryPath));
                }

                return new Response(dynamicTasks, dynamicTaskInputs);
            }
        }
    }
}
