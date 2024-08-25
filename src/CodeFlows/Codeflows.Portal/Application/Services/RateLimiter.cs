using Microsoft.Extensions.Caching.Memory;

namespace Codeflows.Portal.Application.Services
{
    public class RateLimiter(IMemoryCache cache, int limit, TimeSpan timeWindow)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly int _limit = limit;
        private readonly TimeSpan _timeWindow = timeWindow;

        public bool IsRequestAllowed(string key)
        {
            var count = _cache.Get<int>(key);

            if (count >= _limit)
            {
                return false;
            }

            _cache.Set(key, count + 1, _timeWindow);
            return true;
        }
    }
}
