using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SLGateway.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Repositories
{
    public interface IApiKeyRepository
    {
        Task<bool> Create(ApiKey key);
        Task<bool> RecordAccess(string key);
        Task<bool> Delete(string key);
        Task<ApiKey> Get(string key);
        Task<IEnumerable<ApiKey>> GetKeysForOwner(string ownerName);
        Task<bool> DeleteForOwner(string ownerName);
    }

    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<ApiKeyEntity> _collection;
        private ConcurrentDictionary<string, ApiKey> _apiKeys = new ConcurrentDictionary<string, ApiKey>();

        public ApiKeyRepository(ILogger<ApiKeyRepository> logger, IMongoDataSource mongoDataSource)
        {
            _logger = logger;
            _collection = mongoDataSource.Database.GetCollection<ApiKeyEntity>("ApiKeys");
        }

        public async Task<IEnumerable<ApiKey>> GetKeysForOwner(string ownerName)
        {
            var cursor = await _collection.FindAsync(k => k.UserId == ownerName);
            var result = await cursor.ToListAsync();
            return result.Select(x => x.ToApiKey()).ToList();
        }

        public async Task<ApiKey> Get(string key)
        {
            var cursor = await _collection.FindAsync(k => k.Key == key);
            var result = await cursor.FirstOrDefaultAsync();
            return result.ToApiKey();
        }

        public async Task<bool> RecordAccess(string key)
		{
            var result = await _collection.UpdateOneAsync(k => k.Key == key, Builders<ApiKeyEntity>.Update.Set(k => k.LastAccessDate, DateTime.UtcNow));

            if (result.IsAcknowledged)
			{
                _logger.LogTrace("Recorded key access {apiKey} at {time}", key, DateTime.UtcNow);
            }

            return result.IsAcknowledged;
		}

        public async Task<bool> Create(ApiKey key)
        {
            if (string.IsNullOrEmpty(key.Key))
            {
                return false;
            }

            if (_apiKeys.ContainsKey(key.Key))
            {
                return false;
            }

            await _collection.InsertOneAsync(key.ToEntity());

            return true;
        }

        public async Task<bool> Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var result = await _collection.DeleteOneAsync(o => o.Key == key);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteForOwner(string ownerName)
        {
            if (string.IsNullOrEmpty(ownerName))
            {
                return false;
            }

            var result = await _collection.DeleteManyAsync(o => o.UserId == ownerName);
            return result.IsAcknowledged;
        }
    }
}
