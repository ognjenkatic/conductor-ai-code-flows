using System.ComponentModel.DataAnnotations;
using Codeflows.Csharp.Common.Configuration;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Codeflows.Csharp.Quality.Workers
{
    public partial record GetProjectFileLocations : IRequest<GetProjectFileLocations.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        public record Response(List<string> ProjectFilePaths, string ProjectType);

        [OriginalName("detect_projects_csharp")]
        public partial class Handler() : TaskRequestHandler<GetProjectFileLocations, Response>
        {
            public override Task<Response> Handle(
                GetProjectFileLocations request,
                CancellationToken cancellationToken
            )
            {
                var globalRepoDirectoryInfo = new DirectoryInfo(
                    Path.Join(StorageConfiguration.GlobalRootDirectoryPath, request.RepositoryPath)
                );

                if (!globalRepoDirectoryInfo.Exists)
                {
                    throw new InvalidOperationException(
                        "No directory exists at specified global repository path"
                    );
                }

                var solutionPaths =
                    Directory
                        .EnumerateFiles(
                            globalRepoDirectoryInfo.FullName,
                            "*.sln",
                            SearchOption.AllDirectories
                        )
                        .ToList();

                return Task.FromResult(new Response(solutionPaths, "csharp"));
            }
        }
    }
}