namespace Codeflows.Portal.Application.Services
{
    public class RepositoryWhitelist(string[] whitelist)
    {
        public readonly string[] WhitelistedRepos = whitelist;

        public bool IsRepositoryWhitelisted(string url) => WhitelistedRepos.Contains(url);
    }
}
