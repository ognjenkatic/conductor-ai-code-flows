using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Codeflows.Csharp.Common.Configuration;
using Codeflows.Csharp.Quality.DTOs;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using System;

namespace Codeflows.Csharp.Quality.Workers
{
    public record AnalyseCode : IRequest<AnalyseCode.Response>
    {
        [Required]
        [MinLength(1)]
        public required string ProjectFilePath { get; set; }

        [Required]
        public required string ProjectId { get; set; }

        public interface IResponse { }

        [OriginalName("cq_analyse")]
        public class Handler(SonarqubeConfiguration sonarqubeConfiguration)
            : TaskRequestHandler<AnalyseCode, IResponse>
        {
            private readonly SonarqubeConfiguration sonarqubeConfiguration = sonarqubeConfiguration;
            private const string DotnetCommand = "dotnet";

            public override async Task<IResponse> Handle(
                AnalyseCode request,
                CancellationToken cancellationToken
            )
            {
                ArgumentException.ThrowIfNullOrEmpty(request.ProjectId);

                var projectFileInfo = new FileInfo(request.ProjectFilePath);

                await RunCommand(
                    DotnetCommand,
                    $"build {projectFileInfo.Name}",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

                await RunCommand(
                    DotnetCommand,
                    $"sonarscanner begin /k:\"{request.ProjectId}\" /d:sonar.host.url=\"{sonarqubeConfiguration.BaseUrl}\"  /d:sonar.token=\"{sonarqubeConfiguration.Token}\"",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

                await RunCommand(
                    DotnetCommand,
                    $"build {projectFileInfo.Name}",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

                await RunCommand(
                    DotnetCommand,
                    $"dotnet sonarscanner end /d:sonar.token=\"{sonarqubeConfiguration.Token}\"",
                    projectFileInfo.Directory!.FullName,
                    cancellationToken
                );

                // HACK: sometimes it takes a while to process the issues for the next tasks to use them
                await Task.Delay(20000, cancellationToken);
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