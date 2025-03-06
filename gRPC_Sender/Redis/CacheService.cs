using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace gRPC_Sender.Redis
{
    public class CacheService: ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _defaultCacheOptions;
        private readonly JsonSerializerOptions _jsonOptions;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
            // Настройки кеширования (можно сделать параметризуемыми)
            _defaultCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // TTL = 10 минут
            };
            // Настройки сериализации/десериализации JSON
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // Игнорируем регистр имен полей
                WriteIndented = false // Убираем лишние пробелы для экономии места
            };
        }

        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null)
        {
            try
            {
                var cacheOptions = options ?? _defaultCacheOptions;
                var jsonData = JsonSerializer.Serialize(value, _jsonOptions);

                await _cache.SetStringAsync(key, jsonData, cacheOptions);

                Console.WriteLine($"[Redis] Данные сохранены: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis] Ошибка записи: {ex.Message}");
            }
        }


        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var jsonData = await _cache.GetStringAsync(key);
                return jsonData != null ? JsonSerializer.Deserialize<T>(jsonData, _jsonOptions) : default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis] Ошибка при чтении из кэша: {ex.Message}");
                return default;
            }
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
