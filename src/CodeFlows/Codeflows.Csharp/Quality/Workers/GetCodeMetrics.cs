using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Codeflows.Csharp.Quality.DTOs;
using Codeflows.Csharp.Quality.Services;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using System;

namespace Codeflows.Csharp.Quality.Workers
{
    public record GetCodeMetrics : IRequest<GetCodeMetrics.Response>
    {
        [Required]
        public required string ProjectId { get; set; }

        public record Response(Dictionary<string, List<CodeFlowIssue>> IssuesByFile);

        [OriginalName("cq_get_metrics")]
        public class Handler(SonarqubeService sonarqubeService)
            : TaskRequestHandler<GetCodeMetrics, Response>
        {
            private readonly SonarqubeService sonarqubeService = sonarqubeService;
            private static readonly string[] blacklistedIssueMessages =
            [
                // This requires rename across repository which is not trivial
                "Make this class name end with",
                // Not sure i understand this one
                "Change the visibility of this constructor to 'protected'"
            ];

            public override async Task<Response> Handle(
                GetCodeMetrics request,
                CancellationToken cancellationToken
            )
            {
                var currentPage = 1;
                var pageSize = 100;
                var shouldLoadMore = true;
                var issuesLoaded = 0;

                var codeFlowIssues = new Dictionary<string, List<CodeFlowIssue>>();

                while (shouldLoadMore)
                {
                    var queryResponse = await sonarqubeService.GetIssues(
                        request.ProjectId,
                        currentPage,
                        pageSize,
                        cancellationToken
                    );

                    queryResponse
                        .Issues.Where(IsNotBlacklistedIssue)
                        .ToList()
                        .ForEach(issue =>
                        {
                            if (
                                !codeFlowIssues.TryGetValue(
                                    issue.Component,
                                    out List<CodeFlowIssue>? value
                                )
                            )
                            {
                                value = ([]);
                                codeFlowIssues.Add(issue.Component, value);
                            }

                            value.Add(
                                new CodeFlowIssue()
                                {
                                    Message = issue.Message,
                                    StartLine = issue.TextRange.StartLine,
                                    EndLine = issue.TextRange.EndLine,
                                    StartPos = issue.TextRange.StartOffset,
                                    EndPos = issue.TextRange.EndOffset
                                }
                            );
                        });

                    issuesLoaded += queryResponse.Issues.Count;

                    shouldLoadMore = issuesLoaded < queryResponse.Total;
                }

                return new Response(codeFlowIssues);
            }

            private bool IsNotBlacklistedIssue(Issue issue) =>
                Array.Exists(blacklistedIssueMessages, message => issue.Message.StartsWith(message));
        }
    }
}