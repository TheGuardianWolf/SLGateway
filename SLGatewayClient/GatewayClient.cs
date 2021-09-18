using System;

namespace SLGatewayClient
{
    public class GatewayClient
    {
        public string ApiKey { get; set; }
        public string GatewayUrl { get; set; }
        public string ObjectId { get; set; }

        public GatewayClient(string objectId, string gatewayUrl, string apiKey)
        {
            ApiKey = apiKey;
            GatewayUrl = gatewayUrl;
            ObjectId = objectId;
        }


    }
}
