namespace Codeflows.Portal.Application.Services
{
    public class RepositoryWhitelist
    {
        private static readonly string[] whitelistedRepos =
        [
            "https://github.com/ognjenkatic/angry-prs"
        ];

        public bool IsRepositoryWhitelisted(string url) => whitelistedRepos.Contains(url);
    }
}
