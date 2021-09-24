using Microsoft.Extensions.Logging;
using SLGatewayCore;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace SLGatewayClient
{
    public class GatewayClient : IDisposable, IAsyncDisposable
    {
        private const string BearerAuthenticationScheme = "Bearer";

        private static HttpClient _pollingHttpClient = StaticHttpClient.GetClient("GatewayPollingClient");

        private static HttpClient _httpClient = StaticHttpClient.GetClient();

        private ILogger? _logger;

        public string ApiKey { get; set; }
        public Uri GatewayUrl { get; set; }

        private readonly object _eventPollingLock = new object();
        private Task? EventPollingTask { get; set; }
        private CancellationTokenSource? EventPollingCancellationTokenSource { get; set; }

        static GatewayClient()
        {
            _pollingHttpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        public GatewayClient(string gatewayUrl, string apiKey, ILogger? logger)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(gatewayUrl))
            {
                throw new ArgumentException("One or more arguments are not valid values");
            }

            ApiKey = apiKey;
            GatewayUrl = new Uri(gatewayUrl);
        }

        public event EventHandler<ObjectEvent>? OnEventReceived;

        private async Task EventPollingWorker()
        {
            Exception? exception = null;

            if (EventPollingCancellationTokenSource != null)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, $"{GatewayUrl}api/events/longpoll");
                    request.Headers.Authorization = new AuthenticationHeaderValue(BearerAuthenticationScheme, ApiKey);
                    var response = await _pollingHttpClient.SendAsync(request);

                    if (EventPollingCancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    exception = ex;
                    _logger?.LogTrace(ex, "Event polling was cancelled");
                }
                catch (Exception ex)
                {
                    exception = ex;
                    _logger?.LogTrace(ex, "Event polling was disrupted by exception");
                }
                finally
                {
                    if (exception == null || !(exception is TaskCanceledException))
                    {
                        EventPollingTask = Task.Run(EventPollingWorker, EventPollingCancellationTokenSource.Token);
                    }
                }
            }
        }

        public bool EventPolling 
        { 
            get
            {
                lock (_eventPollingLock)
                {
                    return EventPollingTask != null;
                }
            }
            set
            {
                lock (_eventPollingLock)
                {
                    if (value)
                    {
                        if (EventPollingTask == null)
                        {
                            EventPollingCancellationTokenSource = new CancellationTokenSource();
                            EventPollingTask = Task.Run(EventPollingWorker, EventPollingCancellationTokenSource.Token);
                        }
                    }
                    else
                    {
                        if (EventPollingTask != null)
                        {
                            Task.Run(StopEventPollingAsync);

                            EventPollingTask = null;
                            EventPollingCancellationTokenSource = null;
                        }
                    }
                }
            }
        }

        public async Task<CommandEventResponse> SendCommandAsync(Guid objectId, CommandEvent commandEvent)
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
                var data = await response.Content.ReadFromJsonAsync<dynamic>();
                return new CommandEventResponse
                {
                    Data = data,
                    HttpStatusCode = (int)response.StatusCode
                };
            }

            return new CommandEventResponse
            {
                Data = null,
                HttpStatusCode = (int)response.StatusCode
            };
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await StopEventPollingAsync();
            EventPolling = false;
        }

        private async Task StopEventPollingAsync()
        {
            var task = EventPollingTask;
            var tokenSource = EventPollingCancellationTokenSource;

            tokenSource?.Cancel();
            if (task != null)
            {
                await task;
            }
            tokenSource?.Dispose();
        }
    }
}
