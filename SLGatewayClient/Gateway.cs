using Microsoft.Extensions.Logging;
using SLGatewayClient.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SLGatewayClient
{
    public class GatewayConfiguration
    {
        public string GatewayUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public ILogger? Logger { get; set; }
    }

    public class Gateway
    {
        public string GatewayUrl { get; }
        public string ApiKey { get; private set; }
        private readonly GatewayClient _client;
        private readonly ILogger? _logger;

        public Gateway(GatewayConfiguration config)
        {
            if (string.IsNullOrEmpty(config.GatewayUrl))
            {
                throw new ArgumentException("Invalid configuration");
            }

            ApiKey = config.ApiKey;
            GatewayUrl = config.GatewayUrl;
            _logger = config.Logger;
            _client = new GatewayClient(config.GatewayUrl, config.ApiKey, config.Logger);
        }

        public SLObject UseObject(Guid objectId)
        {
            if (string.IsNullOrEmpty(ApiKey))
			{
                throw new InvalidOperationException("Api key cannot be empty when using SLOs");
			}

            return new SLObject(objectId, _client, _logger);
        }

        public void UpdateApiKey(string apiKey)
		{
            ApiKey = apiKey;
            _client.ApiKey = apiKey;
		}

        public async Task<GatewayStatus> CheckConnection(string apiKey)
		{
            var status = await _client.Ping(apiKey);

            GatewayStatus statusFlags = 0;

            if ((int) status < 400 && (int) status >= 200)
			{
                statusFlags |= GatewayStatus.Available;
			}
            else
			{
                statusFlags |= GatewayStatus.Unavailable;
			}
            
            if (status == System.Net.HttpStatusCode.Forbidden || status == System.Net.HttpStatusCode.Unauthorized)
			{
                statusFlags |= GatewayStatus.InvalidApiKey | GatewayStatus.Available;
			}

            return statusFlags;
		}
    }
}
