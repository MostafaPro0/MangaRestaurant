using MangaRestaurant.Core.Service;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MangaRestaurant.Service
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDatabase _database;
        public ResponseCacheService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }
        public async Task CasheResponseAsync(string CasheKey, object Response, TimeSpan ExpireTime)
        {
            if (Response is null) return;
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var serializedResponse = JsonSerializer.Serialize(Response, options);
            await _database.StringSetAsync(CasheKey, serializedResponse, ExpireTime);
        }

        public async Task<string?> GetCashedResponse(string CasheKey)
        {
            var cashedResponse = await _database.StringGetAsync(CasheKey);
            if (cashedResponse.IsNullOrEmpty) return null;
            return cashedResponse;
        }
    }
}
