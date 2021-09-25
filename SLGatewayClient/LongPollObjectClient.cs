using Microsoft.Extensions.Logging;
using SLGatewayCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SLGatewayClient
{
    public class LongPollObjectClient : IDisposable, IAsyncDisposable
    {
        public Guid ObjectId { get; }

        private readonly GatewayClient _client;
        private readonly ILogger? _logger;
        private readonly object _eventPollingLock = new object();
        private Task? EventPollingTask { get; set; }
        private CancellationTokenSource? EventPollingCancellationTokenSource { get; set; }

        public event EventHandler<ObjectEvent>? OnEventReceived;

        public LongPollObjectClient(Guid objectId, GatewayClient client, ILogger? logger)
        {
            ObjectId = objectId;
            _client = client;
            _logger = logger;
        }

        private async Task EventPollingWorker()
        {
            Exception? exception = null;

            _logger?.LogTrace("Event polling cycle");
            if (EventPollingCancellationTokenSource != null)
            {
                try
                {
                    var response = await _client.LongPollAsync(ObjectId);

                    if (response.IsSuccessStatusCode)
                    {
                        var events = response.Data;
                        if (events != null)
                        {
                            foreach (var evt in events)
                            {
                                OnEventReceived?.Invoke(this, evt);
                            }
                        }
                    }
                    else
                    {
                        _logger?.LogError("Event polling error with status: {statusCode}", response.HttpStatusCode);
                    }

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

        public bool Enabled
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

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await StopEventPollingAsync();
            Enabled = false;
        }
    }
}
