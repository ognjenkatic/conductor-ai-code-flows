using System.ComponentModel.DataAnnotations;
using System.Text;
using CodeFlows.GenAi.Common.Util;
using CodeFlows.GenAi.OpenAi.DTOs;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Chat;

namespace CodeFlows.GenAi.OpenAi.Workers
{
    public record RefactorProject : IRequest<RefactorProject.Response>
    {
        [Required]
        [MinLength(1)]
        public required string ProjectFilePath { get; set; }

        [Required]
        public required Dictionary<string, List<CodeFlowIssue>> IssuesByFile { get; set; }

        [Required]
        public required int MaximumParallelPrompts { get; set; }

        public record Response
        {
            public required List<string> RefactoredFiles { get; set; }
        }

        [OriginalName("ai_refactor_project")]
        public class Handler(OpenAIAPI openAiClient, ILogger<Handler> logger)
            : TaskRequestHandler<RefactorProject, Response>
        {
            private const string fileNamePlaceholder = "{filename}";
            private const string issuesPlaceholder = "{issues}";
            private const string sourcecodePlaceholder = "{sourcecode}";

            public override async Task<Response> Handle(
                RefactorProject request,
                CancellationToken cancellationToken
            )
            {
                var refactorPrompt = EmbeddedResourceUtil.ReadResource(
                    typeof(RefactorProject).Assembly,
                    "CodeFlows.GenAi.OpenAi.Resources.RefactorPrompt.txt"
                );

                var maximumPrompts = Math.Max(Math.Min(request.MaximumParallelPrompts, 5), 1);

                if (request.IssuesByFile.Count == 0)
                {
                    return new Response() { RefactoredFiles = new() };
                }

                var projectDir = new FileInfo(request.ProjectFilePath).Directory!.FullName;

                var processedPrompts = 0;

                while (processedPrompts != request.IssuesByFile.Count)
                {
                    var filesToProcess = request
                        .IssuesByFile.Skip(processedPrompts)
                        .Take(maximumPrompts)
                        .ToList();

                    var tasks = new List<Task>();

                    foreach (var fileIssues in filesToProcess)
                    {
                        var sb = new StringBuilder(refactorPrompt);

                        var file = Path.Join(projectDir, fileIssues.Key.Split(":")[1]);
                        var issues = fileIssues.Value;

                        sb = sb.Replace(fileNamePlaceholder, file);
                        sb = sb.Replace(issuesPlaceholder, BuildIssueParagraph(issues));
                        sb = sb.Replace(sourcecodePlaceholder, File.ReadAllText(file));

                        tasks.Add(TryGenerateNewCodeAndWriteToFile(file, sb.ToString()));
                    }

                    await Task.WhenAll(tasks);

                    processedPrompts += filesToProcess.Count;

                    logger.LogDebug(
                        "Refactored {refactorCount} of {totalCount} files",
                        processedPrompts,
                        request.IssuesByFile.Count
                    );
                }

                return new Response() { RefactoredFiles = new() };
            }

            private async Task TryGenerateNewCodeAndWriteToFile(string filePath, string prompt)
            {
                logger.LogDebug("Beginning Refactor: {filePath}", filePath);
                try
                {
                    var convo = openAiClient.Chat.CreateConversation(
                        new ChatRequest
                        {
                            Temperature = 0.5,
                            MaxTokens = 4096,
                            Model = "gpt-4o"
                        }
                    );

                    convo.AppendUserInput(prompt);

                    var newCode = await convo.GetResponseFromChatbotAsync();

                    File.WriteAllText(filePath, newCode);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed Refactoring: {filePath} due to {error}", filePath, ex);
                }
                logger.LogDebug("Finished Refactor: {filePath}", filePath);
            }

            private static string BuildIssueParagraph(List<CodeFlowIssue> issues)
            {
                var sb = new StringBuilder();

                foreach (var issue in issues)
                {
                    sb.AppendLine($"Issue: {issue.Message}");
                    sb.AppendLine($"Starting Line: {issue.StartLine}");
                    sb.AppendLine($"Starting Position: {issue.StartPos}");
                    sb.AppendLine($"Starting Line: {issue.EndLine}");
                    sb.AppendLine($"Starting Line: {issue.EndPos}");
                    sb.AppendLine("----------------------------------------");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }
    }
}
