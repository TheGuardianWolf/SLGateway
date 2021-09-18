using SLGateway.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SLGateway.Repositories
{
    public interface IApiKeyRepository
    {
        ApiKey Get(string key);
        IEnumerable<ApiKey> GetAll();
        bool Create(ApiKey key);
        IEnumerable<ApiKey> GetKeysForOwner(string userId);
        bool Delete(string key);
        void InjectData(IEnumerable<ApiKey> apiKeys);
    }

    public class ApiKeyRepository : IApiKeyRepository
    {
        private ConcurrentDictionary<string, ApiKey> _apiKeys = new ConcurrentDictionary<string, ApiKey>();

        // Used for the moment with json data
        public void InjectData(IEnumerable<ApiKey> apiKeys)
        {
            var newKeys = new ConcurrentDictionary<string, ApiKey>();
            foreach (var key in apiKeys)
            {
                newKeys.TryAdd(key.Key, key);
            }
            _apiKeys = newKeys;
        }

        public IEnumerable<ApiKey> GetKeysForOwner(string ownerName)
        {
            return _apiKeys.Values.Where(x => x.OwnerName == ownerName);
        }

        public ApiKey Get(string key)
        {
            _apiKeys.TryGetValue(key, out var apiKey);
            return apiKey;
        }

        public IEnumerable<ApiKey> GetAll()
        {
            return _apiKeys.Values.ToList();
        }

        public bool Create(ApiKey key)
        {
            if (string.IsNullOrEmpty(key.Key))
            {
                return false;
            }

            if (_apiKeys.ContainsKey(key.Key))
            {
                return false;
            }

            return _apiKeys.TryAdd(key.Key, key);
        }

        public bool Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!_apiKeys.ContainsKey(key))
            {
                return false;
            }

            return _apiKeys.TryRemove(key, out var _);
        }
    }
}
