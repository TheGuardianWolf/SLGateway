using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SLGateway.Data;
using SLGatewayCore.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Repositories
{
    public interface IObjectEventsRepository
    {
        bool Create(Guid objectId, ObjectEvent evt);
        BlockingCollection<ObjectEvent> GetEventQueue(Guid objectId);
    }

    public class ObjectEventsRepository : IObjectEventsRepository, IDisposable
    {
        private ConcurrentDictionary<Guid, BlockingCollection<ObjectEvent>> _events = new ConcurrentDictionary<Guid, BlockingCollection<ObjectEvent>>();

        private readonly int _maxEvents;

        private readonly ILogger _logger;

        public ObjectEventsRepository(IConfiguration configuration, ILogger<ObjectEventsRepository> logger)
        {
            _logger = logger;
            _maxEvents = configuration.GetValue<int>("MaxEventsCache");
        }

        public IEnumerable<ObjectEvent> GetEnumerator(Guid objectId)
        {
            if (!_events.ContainsKey(objectId))
            {
                return Enumerable.Empty<ObjectEvent>();
            }

            _events.TryGetValue(objectId, out var objectEvents);

            return objectEvents.GetConsumingEnumerable();
        }

        public BlockingCollection<ObjectEvent> GetEventQueue(Guid objectId)
        {
            if (!_events.ContainsKey(objectId))
            {
                if (!_events.TryAdd(objectId, new BlockingCollection<ObjectEvent>()))
                {
                    return null;
                }
            }

            _events.TryGetValue(objectId, out var objectEvents);

            return objectEvents;
        }

        public bool Create(Guid objectId, ObjectEvent evt)
        {
            BlockingCollection<ObjectEvent> objectEvents;

            if (!_events.ContainsKey(objectId))
            {
                objectEvents = new BlockingCollection<ObjectEvent>();
                _events.TryAdd(objectId, objectEvents);
            }
            else
            {
                _events.TryGetValue(objectId, out objectEvents);
            }

            if (objectEvents is null)
            {
                return false;
            }

            if (objectEvents.Count > _maxEvents)
            {
                _logger.LogWarning("Discarding event for {objectId} as queue is full", objectId);
                objectEvents.TryTake(out var _);
            }

            return objectEvents.TryAdd(evt);
        }

        public void Dispose()
        {
            foreach (var objectEvent in _events.Values)
            {
                objectEvent.CompleteAdding();
                objectEvent.Dispose();
            }
        }
    }
}
