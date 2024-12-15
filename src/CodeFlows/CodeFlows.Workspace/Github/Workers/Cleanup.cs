using System.ComponentModel.DataAnnotations;
using CodeFlows.Workspace.Common.Configuration;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CodeFlows.Workspace.Github.Workers
{
    public partial record Cleanup : IRequest<Cleanup.Response>
    {
        [Required]
        public required string RepositoryPath { get; set; }

        public interface Response { }

        [OriginalName("gh_cleanup")]
        public partial class Handler : TaskRequestHandler<Cleanup, Response>
        {
            public override Task<Response> Handle(
                Cleanup request,
                CancellationToken cancellationToken
            )
            {
                var directoryInfo = new DirectoryInfo(
                    Path.Join(StorageConfiguration.RootDirectoryPath, request.RepositoryPath)
                );

                if (directoryInfo.Exists)
                {
                    Directory.Delete(directoryInfo.FullName, true);
                }

                return Task.FromResult<Response>(new ResponseImpl());
            }

            private class ResponseImpl : Response { }
        }
    }
}