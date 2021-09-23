using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public class SLGateway
    {
        public string GatewayUrl { get; }

        public SLGateway(string gatewayUrl)
        {
            GatewayUrl = gatewayUrl;
        }


    }
}
