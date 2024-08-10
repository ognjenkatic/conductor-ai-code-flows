using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Codeflows.Csharp.Common.Configuration;
using Codeflows.Csharp.Common.Util;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

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
                var directoryInfo = new DirectoryInfo(
                    Path.Join(StorageConfiguration.RootDirectoryPath, request.RepositoryPath)
                );

                if (!directoryInfo.Exists)
                {
                    throw new InvalidOperationException("No directory exists at specified path");
                }

                var csprojPaths = DirectoryUtils.GetMatchingDirectoryFilePaths(
                    request.RepositoryPath,
                    CsProjectRegex()
                );

                return Task.FromResult(new Response(csprojPaths));
            }

            [GeneratedRegex(".+\\.csproj")]
            private static partial Regex CsProjectRegex();
        }
    }
}
