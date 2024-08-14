using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace CodeFlows.Workspace.Github.Workers
{
    public record CreatePullRequest : IRequest<CreatePullRequest.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        [Required]
        public required string RepositoryNameWithUsername { get; set; }

        [Required]
        public required string PullRequestTitle { get; set; }

        [Required]
        public required string PullRequestDescription { get; set; }

        [Required]
        public required string BaseBranch { get; set; }

        [Required]
        public required string ForkBranch { get; set; }

        public record Response { }

        [OriginalName("gh_create_pr")]
        public class Handler : TaskRequestHandler<CreatePullRequest, Response>
        {
            public override async Task<Response> Handle(
                CreatePullRequest request,
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
                        FileName = "gh",
                        Arguments =
                            $"pr create --base {request.BaseBranch} --head {request.ForkBranch} --repo {request.RepositoryNameWithUsername} --title \"{request.PullRequestTitle}\" --body \"{request.PullRequestDescription}\"\r\n",
                        WorkingDirectory = request.RepositoryPath,
                    }
                };
                gitAddProcess.Start();
                await gitAddProcess.WaitForExitAsync(cancellationToken);

                return new();
            }
        }
    }
}
