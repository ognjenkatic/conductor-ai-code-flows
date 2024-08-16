using System.ComponentModel.DataAnnotations;
using Codeflows.Portal.Infrastructure.Persistence;
using Codeflows.Portal.Infrastructure.Persistence.Entities;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace Codeflows.Portal.Application.Workers
{
    public class UpdateRefactorRun : IRequest<UpdateRefactorRun.Response>
    {
        [Required]
        public required int RefactorRunId { get; set; }

        [Required]
        public required RefactorRunState State { get; set; }

        public class Response { }

        [OriginalName("rr_update_run")]
        public class Handler(CodeflowsDbContext dbContext)
            : TaskRequestHandler<UpdateRefactorRun, Response>
        {
            private readonly CodeflowsDbContext dbContext = dbContext;

            public override async Task<Response> Handle(
                UpdateRefactorRun request,
                CancellationToken cancellationToken
            )
            {
                var refactorRun =
                    await dbContext.RefactorRuns.FindAsync(
                        [request.RefactorRunId],
                        cancellationToken: cancellationToken
                    )
                    ?? throw new InvalidOperationException(
                        "Could not find refactor run by provided id"
                    );

                refactorRun.State = request.State;

                await dbContext.SaveChangesAsync(cancellationToken);

                return new Response();
            }
        }
    }
}
