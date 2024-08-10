using System.Net.Http.Json;
using Codeflows.Csharp.Quality.DTOs;

namespace Codeflows.Csharp.Quality.Services
{
    public class SonarqubeService(HttpClient httpClient)
    {
        private readonly HttpClient httpClient = httpClient;

        public async Task<IssuesResponse> GetIssues(
            string componentId,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentException.ThrowIfNullOrEmpty(componentId);

            var resp = await httpClient.GetAsync(
                $"api/issues/search?components={componentId}",
                cancellationToken
            );

            var respstr = await resp.Content.ReadAsStringAsync();

            return await httpClient.GetFromJsonAsync<IssuesResponse>(
                    $"api/issues/search?components={componentId}",
                    cancellationToken
                ) ?? throw new InvalidOperationException("Could not fetch issues");
        }
    }
}
