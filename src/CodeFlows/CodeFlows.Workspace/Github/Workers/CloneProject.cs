using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CodeFlows.Workspace.Common.Configuration;
using CodeFlows.Workspace.Common.Util;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using LibGit2Sharp;
using MediatR;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace CodeFlows.Workspace.Github.Workers
{
    public partial record CloneProject : IRequest<CloneProject.Response>
    {
        [Required]
        [Url]
        public required string RepositoryUrl { get; set; }

        [Required]
        public required string RepositoryPath { get; set; }

        public record Response(
            string RepositoryPath,
            int NumberOfFilesInRepository,
            long SizeOfRepositoryInBytes,
            string RepositoryName,
            string RepositoryOwner,
            string DefaultBranch,
            string BranchName
        );

        [OriginalName("gh_clone_repo")]
        public partial class Handler(
            GitHubClient githubClient,
            UsernamePasswordCredentials gitCredentials
        ) : TaskRequestHandler<CloneProject, Response>
        {
            private readonly GitHubClient githubClient = githubClient;
            private readonly UsernamePasswordCredentials gitCredentials = gitCredentials;

            public override async Task<Response> Handle(
                CloneProject request,
                CancellationToken cancellationToken
            )
            {
                var (Owner, Name) =
                    ParseRepoUrl(request.RepositoryUrl)
                    ?? throw new InvalidOperationException(
                        "Could not parse repository owner and name from url"
                    );

                var directoryInfo = new DirectoryInfo(
                    Path.Join(StorageConfiguration.RootDirectoryPath, request.RepositoryPath)
                );

                if (directoryInfo.Exists)
                {
                    throw new InvalidOperationException(
                        $"Oops, somehow the destination directory already exists"
                    );
                }
                else
                {
                    directoryInfo.Create();
                }

                var originalRepo = await githubClient.Repository.Get(Owner, Name);

                // Check if the repository is already forked
                Octokit.Repository forkedRepo;
                try
                {
                    forkedRepo = await githubClient.Repository.Get(
                        githubClient.User.Current().Result.Login,
                        Name
                    );
                }
                catch (Octokit.NotFoundException)
                {
                    // If not found, create a new fork
                    forkedRepo = await githubClient.Repository.Forks.Create(
                        Owner,
                        Name,
                        new NewRepositoryFork()
                    );
                }

                // Clone the forked repository
                Repository.Clone(forkedRepo.CloneUrl, directoryInfo.FullName);

                using var repo = new LibGit2Sharp.Repository(directoryInfo.FullName);

                // If the repository was already forked, sync it with the upstream repository
                if (forkedRepo.Owner.Login != Owner)
                {
                    // Add the original repository as the upstream remote
                    var remote = repo.Network.Remotes.Add("upstream", originalRepo.CloneUrl);

                    // Fetch the latest changes from the upstream
                    repo.Network.Fetch(remote.Name, new string[0], new FetchOptions(), null);

                    // Find the upstream branch to merge
                    var upstreamBranch = repo.Branches[
                        $"refs/remotes/{remote.Name}/{originalRepo.DefaultBranch}"
                    ];

                    // Merge the upstream branch into the fork's default branch
                    var signature = new LibGit2Sharp.Signature(
                        "Merger",
                        "merger@example.com",
                        DateTimeOffset.Now
                    );
                    var mergeResult = repo.Merge(upstreamBranch, signature);

                    if (mergeResult.Status == MergeStatus.FastForward)
                    {
                        // Push the changes to the forked repository
                        var remoteOrigin = repo.Network.Remotes["origin"];
                        var pushOptions = new PushOptions
                        {
                            CredentialsProvider = (url, user, cred) => gitCredentials
                        };
                        repo.Network.Push(
                            remoteOrigin,
                            @"refs/heads/" + repo.Head.FriendlyName,
                            pushOptions
                        );
                    }
                    if (mergeResult.Status == MergeStatus.Conflicts)
                    {
                        throw new InvalidOperationException(
                            "Merge conflicts occurred while syncing the fork."
                        );
                    }
                }

                var branch = repo.CreateBranch(
                    $"autorefactor/{originalRepo.DefaultBranch}/{StringUtils.GetRandomString(10)}"
                );

                Commands.Checkout(repo, branch);

                if (!Path.Exists(Path.Join(directoryInfo.FullName, ".git")))
                {
                    throw new InvalidOperationException("Cloning repository failed");
                }

                var repositoryMetadata = DirectoryUtils.GetDirectoryMetadata(
                    directoryInfo.FullName
                );

                var repositoryName = request.RepositoryUrl.Split("/").Last();

                if (!RepositoryNameRegex().IsMatch(repositoryName))
                {
                    throw new InvalidOperationException("Invalid repository name");
                }

                return new Response(
                    request.RepositoryPath,
                    repositoryMetadata.NumberOfFiles,
                    repositoryMetadata.SizeInBytes,
                    Name,
                    Owner,
                    originalRepo.DefaultBranch,
                    branch.FriendlyName
                );
            }

            [GeneratedRegex(@"[A-z0-9_-]+")]
            private static partial Regex RepositoryNameRegex();
        }

        static (string Owner, string Name)? ParseRepoUrl(string repoUrl)
        {
            var match = RepoRegex().Match(repoUrl);
            if (match.Success)
            {
                string owner = match.Groups["owner"].Value;
                string repoName = match.Groups["name"].Value;
                return (owner, repoName);
            }
            else
            {
                return null;
            }
        }

        [GeneratedRegex(@"https:\/\/github\.com\/(?<owner>[^\/]+)\/(?<name>[^\/]+)(\/|$)")]
        private static partial Regex RepoRegex();
    }
}
