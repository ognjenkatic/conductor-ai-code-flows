using System.ComponentModel.DataAnnotations;
using CodeFlows.Workspace.Common.Configuration;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using LibGit2Sharp;
using MediatR;
using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;

namespace CodeFlows.Workspace.Github.Workers
{
    public record CheckForPullRequest : IRequest<CheckForPullRequest.Response>
    {
        [Required]
        public required string RepositoryOwner { get; set; }

        [Required]
        public required string RepositoryName { get; set; }

        public record Response
        {
            public bool HasOpenPullRequests { get; set; }
        }

        [OriginalName("gh_check_for_prs")]
        public class Handler(
            GitHubClient githubClient
        ) : TaskRequestHandler<CheckForPullRequest, Response>
        {
            private readonly GitHubClient githubClient = githubClient;

            public override async Task<Response> Handle(
                CheckForPullRequest request,
                CancellationToken cancellationToken
            )
            {
                var hasOpenPullRequests = await CurrentUserHasOpenPullRequests(
                    request.RepositoryOwner,
                    request.RepositoryName
                );

                return new Response { HasOpenPullRequests = hasOpenPullRequests };
            }

            private async Task<bool> CurrentUserHasOpenPullRequests(
                string repositoryOwner,
                string repositoryName
            )
            {
                var user = await githubClient.User.Current();
                var hasMore = true;
                var startPage = 1;
                var pageSize = 50;

                while (hasMore)
                {
                    var pullRequests = await githubClient.PullRequest.GetAllForRepository(
                        repositoryOwner,
                        repositoryName,
                        new PullRequestRequest() { State = ItemStateFilter.Open },
                        new ApiOptions { StartPage = startPage, PageSize = pageSize, }
                    );

                    if (pullRequests.Any(pr => pr.User.Login == user.Login))
                    {
                        return true;
                    }

                    hasMore = pullRequests.Count >= pageSize;
                    startPage++;
                }

                return false;
            }
        }
    }
}