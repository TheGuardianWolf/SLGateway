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
        void InjectData(IEnumerable<ApiKey> apiKeys);
    }

    public class ApiKeyRepository : IApiKeyRepository
    {
        private ConcurrentBag<ApiKey> _apiKeys = new ConcurrentBag<ApiKey>();

        // Used for the moment with json data
        public void InjectData(IEnumerable<ApiKey> apiKeys)
        {
            var newKeys = new ConcurrentBag<ApiKey>();
            foreach (var key in apiKeys)
            {
                newKeys.Add(key);
            }
            _apiKeys = newKeys;
        }

        public IEnumerable<ApiKey> GetKeysForOwner(string ownerName)
        {
            return _apiKeys.Where(x => x.OwnerName == ownerName);
        }

        public ApiKey Get(string key)
        {
            return _apiKeys.FirstOrDefault(x => x.Key.Equals(key, StringComparison.Ordinal));
        }

        public IEnumerable<ApiKey> GetAll()
        {
            return _apiKeys.ToList();
        }

        public bool Create(ApiKey key)
        {
            if (_apiKeys.FirstOrDefault(x => x.Key == key.Key) is not null)
            {
                return false;
            }

            _apiKeys.Add(key);
            return true;
        }
    }
}
