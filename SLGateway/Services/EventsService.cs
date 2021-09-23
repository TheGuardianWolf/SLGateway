using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SLGateway.Data;
using SLGateway.Repositories;
using SLGatewayCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SLGateway.Services
{
    public interface IEventsService
    {
        Task<IEnumerable<ObjectEvent>> WaitForObjectEvents(Guid objectId, int waitForMs);
        bool AddEventForObject(Guid objectId, ObjectEvent evt);
        Task<CommandEventResponse> PushEventToObject(Guid objectId, CommandEvent evt);
    }

    public class EventsService : IEventsService
    {
        private readonly ILogger _logger;
        private readonly IObjectRegistrationService _objectRegistrationService;
        private readonly IObjectEventsRepository _objectEventsRepository;
        private readonly HttpClient _httpClient;

        public EventsService(ILogger<EventsService> logger, IObjectRegistrationService objectRegistrationService, IHttpClientFactory httpClientFactory, IObjectEventsRepository objectEventsRepository)
        {
            this._objectRegistrationService = objectRegistrationService;
            _objectEventsRepository = objectEventsRepository;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<IEnumerable<ObjectEvent>> WaitForObjectEvents(Guid objectId, int waitForMs)
        {
            if (!_objectRegistrationService.IsRegistered(objectId))
            {
                return null;
            }

            var queue = _objectEventsRepository.GetEventQueue(objectId);

            if (queue == null)
            {
                return null;
            }

            return await Task.Run(() =>
            {
                var events = new List<ObjectEvent>();
                var hasItem = queue.TryTake(out var firstItem, waitForMs);
                if (hasItem)
                {
                    events.Add(firstItem);
                    if (queue.Count > 0)
                    {
                        for (var i = 0; i < queue.Count; i++)
                        {
                            var success = queue.TryTake(out var item, 100);
                            if (success)
                            {
                                events.Add(item);
                            }
                        }
                    }
                }
                return events;
            });
        }

        public bool AddEventForObject(Guid objectId, ObjectEvent evt)
        {
            if (!_objectRegistrationService.IsRegistered(objectId))
            {
                return false;
            }

            return _objectEventsRepository.Create(objectId, evt);
        }

        public async Task<CommandEventResponse> PushEventToObject(Guid objectId, CommandEvent evt)
        {
            var obj = _objectRegistrationService.GetObject(objectId);
            if (obj is null)
            {
                return null;
            }

            var responseContent = new List<dynamic> { (int)evt.Code };
            responseContent.AddRange(evt.Args);

            using var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = JsonContent.Create(responseContent),
                RequestUri = new Uri(obj.Url)
            };
            // This doesn't actually do anything
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(ApiKeyAuthenticationDefaults.BearerAuthenticationScheme, obj.Token);

            // Unfortunately SL can't read the auth header so we use this instead
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("SLOToken", obj.Token));
       
            var response = await _httpClient.SendAsync(requestMessage);

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
    }
}
