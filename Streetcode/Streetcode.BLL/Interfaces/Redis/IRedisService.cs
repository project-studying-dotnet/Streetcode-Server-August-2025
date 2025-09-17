using Streetcode.BLL.Services.Redis;

namespace Streetcode.BLL.Interfaces.Redis
{
    public interface IRedisService<T>
        where T : class
    {
        public Task<T?> GetAsync(string key, CancellationToken cancellationToken);
        public Task SetAsync(string key, T value, TimeToLiveOption option, int time, CancellationToken cancellationToken);
        public Task DeleteAsync(string key, CancellationToken cancellationToken);
        public Task DeleteAsync(IEnumerable<string> keys, CancellationToken cancellationToken);
    }
}
