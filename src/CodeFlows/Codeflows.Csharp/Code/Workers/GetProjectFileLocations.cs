using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Codeflows.Csharp.Common.Configuration;
using Codeflows.Csharp.Common.Util;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using Microsoft.VisualBasic.FileIO;

namespace Codeflows.Csharp.Quality.Workers
{
    public partial record GetProjectFileLocations : IRequest<GetProjectFileLocations.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        public record Response(List<string> ProjectFilePaths);

        [OriginalName("co_get_csproj_locations")]
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

                var myRepoDirectoryPath = Path.Join(
                    StorageConfiguration.MyRootDirectoryPath,
                    StringUtils.GetRandomString(32)
                );

                if (Path.Exists(myRepoDirectoryPath))
                {
                    throw new InvalidOperationException(
                        $"Oops, somehow the destination directory already exists"
                    );
                }

                FileSystem.CopyDirectory(globalRepoDirectoryInfo.FullName, myRepoDirectoryPath);

                var csprojPaths = DirectoryUtils.GetMatchingDirectoryFilePaths(
                    myRepoDirectoryPath,
                    CsProjectRegex()
                );

                return Task.FromResult(new Response(csprojPaths));
            }

            [GeneratedRegex(".+\\.csproj")]
            private static partial Regex CsProjectRegex();
        }
    }
}
