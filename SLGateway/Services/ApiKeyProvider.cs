using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using SLGateway.Repositories;
using System;
using System.Threading.Tasks;

namespace SLGateway.Services
{
	public class ApiKeyProvider : IApiKeyProvider
	{
		private readonly ILogger _logger;
		private readonly IApiKeyRepository _apiKeyRepository;

		public ApiKeyProvider(ILogger<IApiKeyProvider> logger, IApiKeyRepository apiKeyRepository)
		{
			_logger = logger;
			_apiKeyRepository = apiKeyRepository;
		}

		public Task<IApiKey> ProvideAsync(string key)
		{
			try
			{
                return Task.FromResult((IApiKey)_apiKeyRepository.Get(key));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				throw;
			}
		}
	}
}
