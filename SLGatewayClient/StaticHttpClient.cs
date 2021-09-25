using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace SLGatewayClient
{
    static internal class StaticHttpClient
    {
        private static Dictionary<string, HttpClient> _httpClientStore = new Dictionary<string, HttpClient>();
        public static HttpClient Instance { get; } = new HttpClient();

        static StaticHttpClient()
        {
#if DEBUG
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif
        }

        public static HttpClient GetClient(string clientName = "")
        {
            if (string.IsNullOrEmpty(clientName))
            {
                return Instance;
            }
            else
            {
                if (!_httpClientStore.ContainsKey(clientName))
                {
                    _httpClientStore[clientName] = new HttpClient();
                }

                return _httpClientStore[clientName];
            }
        }
    }

}
