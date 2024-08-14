using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace CodeFlows.Workspace.Github.Workers
{
    public record CommitProjectChanges : IRequest<CommitProjectChanges.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        [Required]
        public required string CommitMessage { get; set; }

        public record Response { }

        [OriginalName("gh_commit_change")]
        public class Handler : TaskRequestHandler<CommitProjectChanges, Response>
        {
            public override async Task<Response> Handle(
                CommitProjectChanges request,
                CancellationToken cancellationToken
            )
            {
                if (!Directory.Exists(Path.Join(request.RepositoryPath, ".git")))
                {
                    throw new InvalidOperationException("Repository with .git directory not found");
                }

                var gitAddProcess = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "git",
                        Arguments = $"add .",
                        WorkingDirectory = request.RepositoryPath,
                    }
                };
                gitAddProcess.Start();
                await gitAddProcess.WaitForExitAsync(cancellationToken);

                var gitCommitProcess = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "git",
                        Arguments = $"commit -m {request.CommitMessage}",
                        WorkingDirectory = request.RepositoryPath,
                    }
                };
                gitCommitProcess.Start();
                await gitCommitProcess.WaitForExitAsync(cancellationToken);

                return new();
            }
        }
    }
}
