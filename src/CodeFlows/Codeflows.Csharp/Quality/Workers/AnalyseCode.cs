using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Codeflows.Csharp.Common.Configuration;
using Codeflows.Csharp.Quality.DTOs;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace Codeflows.Csharp.Quality.Workers
{
    public record AnalyseCode : IRequest<AnalyseCode.Response>
    {
        [Required]
        public required List<string> ProjectFilePaths { get; set; }

        [Required]
        public required string ProjectId { get; set; }

        public record Response { }

        [OriginalName("cq_analyse")]
        public class Handler(SonarqubeConfiguration sonarqubeConfiguration)
            : TaskRequestHandler<AnalyseCode, Response>
        {
            private readonly SonarqubeConfiguration sonarqubeConfiguration = sonarqubeConfiguration;

            public override async Task<Response> Handle(
                AnalyseCode request,
                CancellationToken cancellationToken
            )
            {
                ArgumentException.ThrowIfNullOrEmpty(request.ProjectId);

                if (request.ProjectFilePaths.Count == 0)
                {
                    return new Response();
                }

                var projectFileInfos = request
                    .ProjectFilePaths.Select(p => new FileInfo(p))
                    .ToList();

                // Make sure all projects build before doing anything
                foreach (var projectPath in projectFileInfos)
                {
                    await RunCommand(
                        "dotnet",
                        $"build {projectPath.Name}",
                        projectPath.Directory!.FullName,
                        cancellationToken
                    );
                }

                foreach (var projectPath in projectFileInfos)
                {
                    await RunCommand(
                        "dotnet",
                        $"sonarscanner begin /k:\"{request.ProjectId}\" /d:sonar.host.url=\"{sonarqubeConfiguration.BaseUrl}\"  /d:sonar.token=\"{sonarqubeConfiguration.Token}\"",
                        projectPath.Directory!.FullName,
                        cancellationToken
                    );

                    await RunCommand(
                        "dotnet",
                        $"build {projectPath.Name}",
                        projectPath.Directory!.FullName,
                        cancellationToken
                    );

                    await RunCommand(
                        "dotnet",
                        $"dotnet sonarscanner end /d:sonar.token=\"{sonarqubeConfiguration.Token}\"",
                        projectPath.Directory!.FullName,
                        cancellationToken
                    );
                }

                return new Response();
            }

            private static async Task RunCommand(
                string command,
                string arguments,
                string workingDirectory,
                CancellationToken cancellationToken = default
            )
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = command,
                        Arguments = arguments,
                        WorkingDirectory = workingDirectory
                    }
                };
                process.Start();
                await process.WaitForExitAsync(cancellationToken);
            }
        }
    }
}
