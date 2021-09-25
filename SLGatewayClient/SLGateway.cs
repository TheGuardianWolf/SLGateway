using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public class SLGatewayConfiguration
    {
        public string GatewayUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public ILogger? Logger { get; set; }
    }

    public class SLGateway
    {
        public string GatewayUrl { get; }
        private readonly GatewayClient _client;
        private readonly ILogger? _logger;

        public SLGateway(SLGatewayConfiguration config)
        {
            if (string.IsNullOrEmpty(config.GatewayUrl) || string.IsNullOrEmpty(config.ApiKey))
            {
                throw new ArgumentException("Invalid configuration");
            }

            GatewayUrl = config.GatewayUrl;
            _logger = config.Logger;
            _client = new GatewayClient(config.GatewayUrl, config.ApiKey, config.Logger);
        }

        public SLObject UseObject(Guid objectId)
        {
            return new SLObject(objectId, _client, _logger);
        }
    }
}
