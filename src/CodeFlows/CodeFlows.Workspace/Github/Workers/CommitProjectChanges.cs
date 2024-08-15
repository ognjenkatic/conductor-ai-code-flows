using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using CodeFlows.Workspace.Common.Configuration;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using LibGit2Sharp;
using MediatR;

namespace CodeFlows.Workspace.Github.Workers
{
    public record CommitProjectChanges : IRequest<CommitProjectChanges.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        [Required]
        public required string CommitMessage { get; set; }

        public record Response(bool AreThereAnyChanges);

        [OriginalName("gh_commit_change")]
        public class Handler : TaskRequestHandler<CommitProjectChanges, Response>
        {
            public override Task<Response> Handle(
                CommitProjectChanges request,
                CancellationToken cancellationToken
            )
            {
                var directoryInfo = new DirectoryInfo(
                    Path.Join(StorageConfiguration.RootDirectoryPath, request.RepositoryPath)
                );

                if (!Directory.Exists(Path.Join(directoryInfo.FullName, ".git")))
                {
                    throw new InvalidOperationException("Repository with .git directory not found");
                }

                var signature = new Signature("codebot", "codebot@test.com", DateTimeOffset.UtcNow);
                using var repo = new Repository(directoryInfo.FullName);

                var filesToCommit = Directory
                    .GetFiles(directoryInfo.FullName, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains(".sonarqube"))
                    .Select(f => Path.GetRelativePath(directoryInfo.FullName, f))
                    .ToArray();

                Commands.Stage(repo, filesToCommit);

                var changes = repo.Diff.Compare<TreeChanges>(null, true);

                if (changes.Count == 0)
                {
                    return Task.FromResult(new Response(false));
                }

                repo.Commit(request.CommitMessage, signature, signature);

                return Task.FromResult(new Response(true));
            }
        }
    }
}
