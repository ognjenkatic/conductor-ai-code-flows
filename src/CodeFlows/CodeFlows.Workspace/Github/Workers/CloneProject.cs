using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFlows.Workspace.Common.Configuration;
using CodeFlows.Workspace.Common.Util;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace CodeFlows.Workspace.Github.Workers
{
    public record CloneProject : IRequest<CloneProject.Response>
    {
        [Required]
        public required string RepositoryUrl { get; set; }

        public record Response(
            string RepositoryPath,
            int NumberOfFilesInRepository,
            long SizeOfRepositoryInBytes
        );

        [OriginalName("gh_clone_repo")]
        public class Handler(StorageConfiguration storageConfiguration)
            : TaskRequestHandler<CloneProject, Response>
        {
            private readonly StorageConfiguration storageConfiguration = storageConfiguration;

            public override async Task<Response> Handle(
                CloneProject request,
                CancellationToken cancellationToken
            )
            {
                var directoryName = StringUtils.GetRandomString(32);

                var directoryInfo = new DirectoryInfo(
                    Path.Join(storageConfiguration.RootDirectoryPath, directoryName)
                );

                if (directoryInfo.Exists)
                {
                    throw new InvalidOperationException(
                        $"Oops, somehow the destination directory already exists"
                    );
                }
                else
                {
                    directoryInfo.Create();
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "gh",
                        Arguments = $"repo clone {request.RepositoryUrl} .",
                        WorkingDirectory = directoryInfo.FullName,
                    }
                };
                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                if (!Path.Exists(Path.Join(directoryInfo.FullName, ".git")))
                {
                    throw new InvalidOperationException("Cloning repository failed");
                }

                var repositoryMetadata = DirectoryUtils.GetDirectoryMetadata(
                    directoryInfo.FullName
                );

                return new Response(
                    directoryName,
                    repositoryMetadata.NumberOfFiles,
                    repositoryMetadata.SizeInBytes
                );
            }
        }
    }
}
