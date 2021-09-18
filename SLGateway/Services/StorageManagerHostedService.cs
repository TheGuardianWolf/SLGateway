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
        private readonly ILogger _logger;
        private readonly string _storagePath;

        public StorageManagerHostedService(IConfiguration config, ILogger<StorageManagerHostedService> logger, IObjectRegistrationRepository objectRegistrationRepository)
        {
            _objectRegistrationRepository = objectRegistrationRepository;
            _storagePath = Path.GetFullPath(config.GetValue<string>("StorageDirectory"));
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Load object registrations
            var jsonPath = Path.Combine(_storagePath, "objectRegistration.json");
            if (File.Exists(jsonPath))
            {
                _logger.LogTrace("Found objectRegistration store at {path}", jsonPath);
                using var fileStream = File.OpenRead(Path.Combine(_storagePath, "objectRegistration.json"));
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<ObjectRegistration>>(fileStream);
                _objectRegistrationRepository.InjectData(data);
            }
            else
            {
                _logger.LogTrace("No existing objectRegistration at {path}", jsonPath);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Save object registrations
            var jsonPath = Path.Combine(_storagePath, "objectRegistration.json");
            var dir = Directory.CreateDirectory(_storagePath);

            if (dir.Exists)
            {
                _logger.LogTrace("Attempting to save objectRegistration to {path}", jsonPath);
                using var fileStream = File.OpenWrite("objectRegistration.json");
                var data = _objectRegistrationRepository.GetAll();
                await JsonSerializer.SerializeAsync(fileStream, data);
            }
            else
            {
                _logger.LogWarning("Creating storage directory failed for {path}", jsonPath);
            }
        }
    }
}
