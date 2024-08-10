using System.Net.Http.Json;
using Codeflows.Csharp.Quality.DTOs;

namespace Codeflows.Csharp.Quality.Services
{
    public class SonarqubeService(HttpClient httpClient)
    {
        private readonly HttpClient httpClient = httpClient;

        public async Task<IssuesResponse> GetIssues(
            string componentId,
            int pageNumber,
            int pageSize = 100,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentException.ThrowIfNullOrEmpty(componentId);

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Min(Math.Max(pageSize, 1), 500);

            return await httpClient.GetFromJsonAsync<IssuesResponse>(
                    $"api/issues/search?components={componentId}&p={pageNumber}&ps={pageSize}",
                    cancellationToken
                ) ?? throw new InvalidOperationException("Could not fetch issues");
        }

        public async Task<ProjectsResponse> GetProjects(
            List<string> projectKeys,
            int pageNumber,
            int pageSize = 100,
            CancellationToken cancellationToken = default
        )
        {
            projectKeys ??= [];

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Min(Math.Max(pageSize, 1), 500);

            return await httpClient.GetFromJsonAsync<ProjectsResponse>(
                    $"api/projects/search?projects={string.Join(',', projectKeys)}&p={pageNumber}&ps={pageSize}",
                    cancellationToken
                ) ?? throw new InvalidOperationException("Could not fetch projects");
        }

        public async Task CreateProject(
            string name,
            string project,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentException.ThrowIfNullOrEmpty(project);

            var response = await httpClient.PostAsync(
                $"api/projects/create?name={name}&project={project}",
                content: null,
                cancellationToken: cancellationToken
            );

            var ms = response.RequestMessage?.Content?.ReadAsStringAsync();

            var contnet = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Could not create project");
            }
        }
    }
}
