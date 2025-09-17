using Microsoft.Extensions.Caching.Distributed;
using Streetcode.BLL.Interfaces.Redis;

namespace Streetcode.BLL.Services.Redis
{
    public class RedisService<T> : IRedisService<T>
        where T : class
    {
        private readonly IDistributedCache _cache;
        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }

        public async Task DeleteAsync(IEnumerable<string> keys, CancellationToken cancellationToken)
        {
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
        }

        public async Task<T> GetAsync(string key, CancellationToken cancellationToken)
        {
            var cachedItem = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(cachedItem))
            {
                var itemFromCache = System.Text.Json.JsonSerializer.Deserialize<T>(cachedItem);

                return itemFromCache;
            }

            return null;
        }

        public async Task SetAsync(string key, T value, TimeToLiveOption option, int time, CancellationToken cancellationToken)
        {
            var serializedItem = System.Text.Json.JsonSerializer.Serialize(value);

            var cacheEntryOptions = new DistributedCacheEntryOptions();

            var ttl = GetTimeSpan(option, time);

            if (ttl.HasValue)
            {
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = ttl.Value;
            }

            await _cache.SetStringAsync(key, serializedItem, cacheEntryOptions, cancellationToken);
        }

        private static TimeSpan? GetTimeSpan(TimeToLiveOption option, int time)
        {
            return option switch
            {
                TimeToLiveOption.Seconds => TimeSpan.FromSeconds(time),
                TimeToLiveOption.Minutes => TimeSpan.FromMinutes(time),
                TimeToLiveOption.Hours => TimeSpan.FromHours(time),
                TimeToLiveOption.Days => TimeSpan.FromDays(time),
                TimeToLiveOption.Infinity => null,
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
        }
    }
}
