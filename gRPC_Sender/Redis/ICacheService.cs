

using Microsoft.Extensions.Caching.Distributed;

namespace gRPC_Sender.Redis
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null);
    }
}
