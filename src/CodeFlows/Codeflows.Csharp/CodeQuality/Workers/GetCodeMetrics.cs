using System.ComponentModel.DataAnnotations;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace Codeflows.Csharp.CodeQuality.Workers
{
    public record GetCodeMetrics : IRequest<GetCodeMetrics.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        public record Response();

        [OriginalName("cq_get_metrics")]
        public class Handler() : TaskRequestHandler<GetCodeMetrics, Response>
        {
            public override Task<Response> Handle(
                GetCodeMetrics request,
                CancellationToken cancellationToken
            )
            {
                throw new NotImplementedException();
            }
        }
    }
}
