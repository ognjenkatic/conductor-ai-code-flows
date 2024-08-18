namespace Codeflows.Portal.Application.Services
{
    public class RepositoryWhitelist
    {
        public static readonly string[] WhitelistedRepos =
        [
            "https://github.com/ognjenkatic/angry-prs"
        ];

        public bool IsRepositoryWhitelisted(string url) => WhitelistedRepos.Contains(url);
    }
}
