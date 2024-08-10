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
            pageSize = Math.Min(Math.Max(pageSize, 1), 100);

            return await httpClient.GetFromJsonAsync<IssuesResponse>(
                    $"api/issues/search?components={componentId}&p={pageNumber}&ps={pageSize}",
                    cancellationToken
                ) ?? throw new InvalidOperationException("Could not fetch issues");
        }
    }
}
