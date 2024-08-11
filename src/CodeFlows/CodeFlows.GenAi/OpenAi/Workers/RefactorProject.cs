using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFlows.GenAi.Common.Util;
using CodeFlows.GenAi.OpenAi.DTOs;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace CodeFlows.GenAi.OpenAi.Workers
{
    public record RefactorProject : IRequest<RefactorProject.Response>
    {
        [Required]
        [MinLength(1)]
        public required List<string> ProjectFilePaths { get; set; }

        [Required]
        public required Dictionary<string, List<CodeFlowIssue>> IssuesByFile { get; set; }

        [Required]
        public required int MaximumParallelPrompts { get; set; }

        public record Response
        {
            public required List<string> RefactoredFiles { get; set; }
        }

        [OriginalName("ai_refactor_project")]
        public class Handler : TaskRequestHandler<RefactorProject, Response>
        {
            private const string fileNamePlaceholder = "{filename}";
            private const string issuesPlaceholder = "{issues}";
            private const string sourcecodePlaceholder = "{sourcecode}";

            private readonly OpenAIAPI openAiClient;

            public Handler(OpenAIAPI openAiClient)
            {
                this.openAiClient = openAiClient;
            }

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

                if (request.ProjectFilePaths.Count != 1)
                {
                    throw new InvalidCastException(
                        "Currently multiple project paths are not supported, only 1"
                    );
                }

                var projectDir = new FileInfo(request.ProjectFilePaths[0]).Directory!.FullName;

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
                        var file = Path.Join(projectDir, fileIssues.Key.Split(":")[1]);
                        var issues = fileIssues.Value;

                        refactorPrompt = refactorPrompt.Replace(fileNamePlaceholder, file);
                        refactorPrompt = refactorPrompt.Replace(
                            issuesPlaceholder,
                            BuildIssueParagraph(issues)
                        );
                        refactorPrompt = refactorPrompt.Replace(
                            sourcecodePlaceholder,
                            File.ReadAllText(file)
                        );

                        tasks.Add(GenerateNewCodeAndWriteToFile(file, refactorPrompt));
                    }

                    await Task.WhenAll(tasks);

                    processedPrompts += filesToProcess.Count;
                }

                return new Response() { RefactoredFiles = new() };
            }

            private async Task GenerateNewCodeAndWriteToFile(string filePath, string prompt)
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
