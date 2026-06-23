using ConnectHub.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Services
{
    public class CacheService : ICacheService
    {

        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public CacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var json = await _db.StringGetAsync(key);

            return json.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(json!); // deserialize converts from json to object<T>s
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();

            if (keys.Any())
                await _db.KeyDeleteAsync(keys);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value); // serialize converts from object to json in order to be stored in redis
            await _db.StringSetAsync(key, json,expiry?? TimeSpan.FromMinutes(5));
        }

    }
}
