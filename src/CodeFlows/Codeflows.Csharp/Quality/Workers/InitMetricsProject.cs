using System.ComponentModel.DataAnnotations;
using Codeflows.Csharp.Quality.Services;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace Codeflows.Csharp.Quality.Workers
{
    public record InitMetricsProject : IRequest<InitMetricsProject.Response>
    {
        [Required]
        public required string ProjectId { get; set; }

        public record Response(bool AlreadyExists);

        [OriginalName("cq_init_metrics_project")]
        public class Handler(SonarqubeService sonarqubeService)
            : TaskRequestHandler<InitMetricsProject, Response>
        {
            private readonly SonarqubeService sonarqubeService = sonarqubeService;

            public override async Task<Response> Handle(
                InitMetricsProject request,
                CancellationToken cancellationToken
            )
            {
                var projectAlreadyExists = false;

                var existingProjects = await sonarqubeService.GetProjects(
                    [request.ProjectId],
                    1,
                    500,
                    cancellationToken
                );

                if (existingProjects.Components.Count > 1)
                {
                    throw new InvalidOperationException("Multiple projects exist with same id");
                }
                else if (existingProjects.Components.Count == 1)
                {
                    projectAlreadyExists = true;
                }
                else
                {
                    await sonarqubeService.CreateProject(
                        request.ProjectId,
                        request.ProjectId,
                        cancellationToken
                    );
                }

                return new Response(projectAlreadyExists);
            }
        }
    }
}
