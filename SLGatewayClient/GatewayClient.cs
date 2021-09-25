using Microsoft.Extensions.Logging;
using SLGatewayCore;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;

namespace SLGatewayClient
{
    public class GatewayClient
    {
        private const string BearerAuthenticationScheme = "Bearer";

        private static HttpClient _pollingHttpClient = StaticHttpClient.GetClient("LongPollClient", (client) =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        private static HttpClient _httpClient = StaticHttpClient.GetClient();

        private readonly ILogger? _logger;
        public string ApiKey { get; set; }
        public Uri GatewayUrl { get; set; }

        public GatewayClient(string gatewayUrl, string apiKey, ILogger? logger)
        {
            _logger = logger;

            logger.LogTrace("Gateway client initialised with {url} and {apiKey}", gatewayUrl, apiKey);

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(gatewayUrl))
            {
                throw new ArgumentException("One or more arguments are not valid values");
            }

            ApiKey = apiKey;
            GatewayUrl = new Uri(gatewayUrl);
        }

        public async Task<EventResponse<IEnumerable<ObjectEvent>>> LongPollAsync(Guid objectId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{GatewayUrl}api/events/longpoll/{objectId}");
            request.Headers.Authorization = new AuthenticationHeaderValue(BearerAuthenticationScheme, ApiKey);
            var response = await _pollingHttpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadFromJsonAsync<IEnumerable<ObjectEvent>>();
                return new EventResponse<IEnumerable<ObjectEvent>>
                {
                    HttpStatusCode = (int)response.StatusCode,
                    Data = events
                };
            }

            return new EventResponse<IEnumerable<ObjectEvent>>
            {
                HttpStatusCode = (int)response.StatusCode,
                Data = null
            }; ;
        }

        public async Task<EventResponse> SendCommandAsync(Guid objectId, CommandEvent commandEvent) => await SendCommandAsync<object>(objectId, commandEvent);

        public async Task<EventResponse<T>> SendCommandAsync<T>(Guid objectId, CommandEvent commandEvent)
        {
            if (objectId == Guid.Empty)
            {
                throw new ArgumentException("Object id is invalid");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{GatewayUrl}api/events/push/{objectId}");
            request.Headers.Authorization = new AuthenticationHeaderValue(BearerAuthenticationScheme, ApiKey);
            request.Content = JsonContent.Create(commandEvent);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var eventResponse = await response.Content.ReadFromJsonAsync<EventResponse<T>>();
                if (eventResponse != null)
                {
                    return eventResponse;
                }
            }

            return new EventResponse<T>
            {
                Data = default(T),
                HttpStatusCode = (int)response.StatusCode
            };
        }
    }
}
