using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using SLGateway.Data;
using SLGateway.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SLGateway.Services
{
    public interface IApiKeyService
    {
        ApiKey Get(string key);
        IEnumerable<ApiKey> GetForUser(string username);
        ApiKey Create(string commonName, string username, IEnumerable<string> claims);
        bool Invalidate(string key);
    }

    public class ApiKeyService : IApiKeyProvider, IApiKeyService
    {
        private const int apiKeyLength = 20;

        private readonly ILogger _logger;
        private readonly IApiKeyRepository _apiKeyRepository;

        public ApiKeyService(ILogger<IApiKeyProvider> logger, IApiKeyRepository apiKeyRepository)
        {
            _logger = logger;
            _apiKeyRepository = apiKeyRepository;
        }

        public Task<IApiKey> ProvideAsync(string key)
        {
            try
            {
                var apiKey = Get(key);
                _logger.LogTrace("Api key requested: {key}, returned: {apiKey}", key, apiKey?.Key);

                return Task.FromResult((IApiKey)apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public ApiKey Get(string key)
        {
            return _apiKeyRepository.Get(key);
        }

        public IEnumerable<ApiKey> GetForUser(string username)
        {
            return _apiKeyRepository.GetKeysForOwner(username);
        }

        public ApiKey Create(string commonName, string username, IEnumerable<string> scopes)
        {
            var allowedScopes = new string[] { ApiKeyScopes.Client, ApiKeyScopes.Object };
            const string allowableChars = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";

            var acceptedScopes = allowedScopes.Intersect(scopes, StringComparer.OrdinalIgnoreCase);
            if (!acceptedScopes.Any())
            {
                return null;
            }

            // Generate random data
            var rnd = new byte[apiKeyLength];
            RandomNumberGenerator.Fill(rnd);

            // Generate the output string
            var allowable = allowableChars.ToCharArray();
            var l = allowable.Length;
            var chars = new char[apiKeyLength];
            for (var i = 0; i < apiKeyLength; i++)
            {
                chars[i] = allowable[rnd[i] % l];
            }

            var key = "SLGAPI-" + new string(chars);

            var apiKey = new ApiKey
            {
                Key = key,
                CreatedUtc = DateTime.UtcNow,
                Name = commonName,
                UserId = username,
                Scopes = acceptedScopes
            };

            if (!_apiKeyRepository.Create(apiKey))
            {
                return null;
            }

            return apiKey;
        }

        public bool Invalidate(string key)
        {
            return _apiKeyRepository.Delete(key);
        }
    }
}
