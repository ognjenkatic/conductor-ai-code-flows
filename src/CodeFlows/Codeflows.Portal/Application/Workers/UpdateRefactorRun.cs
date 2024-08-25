using System.ComponentModel.DataAnnotations;
using Codeflows.Portal.Infrastructure.Persistence;
using Codeflows.Portal.Infrastructure.Persistence.Entities;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Codeflows.Portal.Application.Workers
{
    public class UpdateRefactorRun : IRequest<UpdateRefactorRun.Response>
    {
        [Required]
        public required int RefactorRunId { get; set; }

        [Required]
        public required RefactorRunState State { get; set; }
        public string? PullRequestUrl { get; set; }
        public string? Note { get; set; }

        public class Response { }

        [OriginalName("rr_update_run")]
        public class Handler : TaskRequestHandler<UpdateRefactorRun, Response>
        {
            private readonly CodeflowsDbContext dbContext;

            public Handler(CodeflowsDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

            public override async Task<Response> Handle(
                UpdateRefactorRun request,
                CancellationToken cancellationToken
            )
            {
                var refactorRun =
                    await dbContext.RefactorRuns.FindAsync(
                        new object[] { request.RefactorRunId },
                        cancellationToken: cancellationToken
                    )
                    ?? throw new InvalidOperationException(
                        "Could not find refactor run by provided id"
                    );

                refactorRun.State = request.State;
                refactorRun.PullRequestUrl = request.PullRequestUrl ?? refactorRun.PullRequestUrl;
                refactorRun.Note = request.Note ?? refactorRun.Note;

                await dbContext.SaveChangesAsync(cancellationToken);

                return new Response();
            }
        }
    }
}