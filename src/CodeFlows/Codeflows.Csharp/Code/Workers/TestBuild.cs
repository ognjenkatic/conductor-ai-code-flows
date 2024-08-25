using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using System;

namespace Codeflows.Csharp.Quality.Workers
{
    public partial record TestBuild : IRequest<TestBuild.Response>
    {
        [Required]
        public required string ProjectFilePath { get; set; }

        public interface Response { }

        [OriginalName("csharp_test_build")]
        public partial class Handler() : TaskRequestHandler<TestBuild, Response>
        {
            public override async Task<Response> Handle(
                TestBuild request,
                CancellationToken cancellationToken
            )
            {
                var fileInfo = new FileInfo(request.ProjectFilePath);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "dotnet",
                        Arguments = $"build {request.ProjectFilePath}",
                        WorkingDirectory = fileInfo.Directory!.FullName
                    }
                };
                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Build returned error");
                }

                return new();
            }
        }
    }
}