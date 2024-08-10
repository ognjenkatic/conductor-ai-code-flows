using System.ComponentModel.DataAnnotations;
using Codeflows.Csharp.Quality.Services;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace Codeflows.Csharp.Quality.Workers
{
    public record GetCodeMetrics : IRequest<GetCodeMetrics.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        public record Response();

        [OriginalName("cq_get_metrics")]
        public class Handler(SonarqubeService sonarqubeService)
            : TaskRequestHandler<GetCodeMetrics, Response>
        {
            private readonly SonarqubeService sonarqubeService = sonarqubeService;

            public override async Task<Response> Handle(
                GetCodeMetrics request,
                CancellationToken cancellationToken
            )
            {
                var issues = await sonarqubeService.GetIssues("test-project", cancellationToken);

                throw new NotImplementedException();
            }
        }
    }
}
