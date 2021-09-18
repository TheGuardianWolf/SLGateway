using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SLGateway.Data;
using SLGateway.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SLGateway.Services
{
    public class StorageManagerHostedService : IHostedService
    {
        private readonly IObjectRegistrationRepository _objectRegistrationRepository;
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly ILogger _logger;
        private readonly string _storagePath;
        private readonly ApiKey _testApiKey;

        public StorageManagerHostedService(IConfiguration config, ILogger<StorageManagerHostedService> logger, IObjectRegistrationRepository objectRegistrationRepository, IApiKeyRepository apiKeyRepository)
        {
            _objectRegistrationRepository = objectRegistrationRepository;
            _apiKeyRepository = apiKeyRepository;
            _storagePath = Path.GetFullPath(config.GetValue<string>("StorageDirectory"));
            _logger = logger;

            var testingApiKeySection = config.GetSection("TestingApiKey");
            if (testingApiKeySection.Exists())
            {
                var apiKey = new ApiKey();
                apiKey.Scopes = null;
                testingApiKeySection.Bind(apiKey);
                _testApiKey = apiKey;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Load API keys
            var apiKeysPath = Path.Combine(_storagePath, "apiKeys.json");
            if (File.Exists(apiKeysPath))
            {
                _logger.LogTrace("Found apiKey store at {path}", apiKeysPath);
                using var fileStream = File.OpenRead(apiKeysPath);
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<ApiKey>>(fileStream);
                _apiKeyRepository.InjectData(data);
            }
            else
            {
                _logger.LogTrace("No existing apiKeys at {path}", apiKeysPath);
            }

            if (_testApiKey is not null && !string.IsNullOrEmpty(_testApiKey.Key))
            {
                if (_apiKeyRepository.Create(_testApiKey))
                {
                    _logger.LogDebug("Testing api key inserted: {apiKey}", _testApiKey.Key);
                }
                else
                {
                    _logger.LogDebug("Testing api key already exists");
                }
            }

            // Load object registrations
            var objectRegistrationPath = Path.Combine(_storagePath, "objectRegistration.json");
            if (File.Exists(objectRegistrationPath))
            {
                _logger.LogTrace("Found objectRegistration store at {path}", objectRegistrationPath);
                using var fileStream = File.OpenRead(objectRegistrationPath);
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<ObjectRegistration>>(fileStream);
                _objectRegistrationRepository.InjectData(data);
            }
            else
            {
                _logger.LogTrace("No existing objectRegistration at {path}", objectRegistrationPath);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var dir = Directory.CreateDirectory(_storagePath);

            // Save API keys
            var apiKeysPath = Path.Combine(_storagePath, "apiKeys.json");
            if (dir.Exists)
            {
                _logger.LogTrace("Found apiKey store at {path}", apiKeysPath);
                using var fileStream = File.OpenWrite(apiKeysPath);
                var data = _apiKeyRepository.GetAll();
                await JsonSerializer.SerializeAsync(fileStream, data);
            }

            // Save object registrations
            var objectRegistrationPath = Path.Combine(_storagePath, "objectRegistration.json");
            if (dir.Exists)
            {
                _logger.LogTrace("Attempting to save objectRegistration to {path}", objectRegistrationPath);
                using var fileStream = File.OpenWrite(objectRegistrationPath);
                var data = _objectRegistrationRepository.GetAll();
                await JsonSerializer.SerializeAsync(fileStream, data);
            }
        }
    }
}
