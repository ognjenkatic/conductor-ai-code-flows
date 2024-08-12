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
        [MinLength(1)]
        public required string ProjectFilePath { get; set; }

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

                var projectFileInfo = new FileInfo(request.ProjectFilePath);

                await RunCommand(
                    "dotnet",
                    $"build {projectFileInfo.Name}",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

                await RunCommand(
                    "dotnet",
                    $"sonarscanner begin /k:\"{request.ProjectId}\" /d:sonar.host.url=\"{sonarqubeConfiguration.BaseUrl}\"  /d:sonar.token=\"{sonarqubeConfiguration.Token}\"",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

                await RunCommand(
                    "dotnet",
                    $"build {projectFileInfo.Name}",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

                await RunCommand(
                    "dotnet",
                    $"dotnet sonarscanner end /d:sonar.token=\"{sonarqubeConfiguration.Token}\"",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

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

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Command {command} returned error");
                }
            }
        }
    }
}
