using SLGateway.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Repositories
{
    public interface IObjectRegistrationRepository
    {
        void InjectData(IEnumerable<ObjectRegistration> registrations);
        ObjectRegistration Get(Guid objectId);
        IEnumerable<ObjectRegistration> GetAll();
        bool Update(ObjectRegistration registration);
        bool Delete(Guid objectId);
    }

    public class ObjectRegistrationRepository : IObjectRegistrationRepository
    {
        private ConcurrentDictionary<Guid, ObjectRegistration> _objects = new ConcurrentDictionary<Guid, ObjectRegistration>();

        // Used for the moment with json data
        public void InjectData(IEnumerable<ObjectRegistration> registrations)
        {
            var objects = new ConcurrentDictionary<Guid, ObjectRegistration>();
            foreach (var registration in registrations)
            {
                objects.TryAdd(registration.Id, registration);
            }
            _objects = objects;
        }

        public IEnumerable<ObjectRegistration> GetAll()
        {
            return _objects.Values.ToList();
        }

        public ObjectRegistration Get(Guid objectId)
        {
            _objects.TryGetValue(objectId, out var val);

            return val;
        }

        public bool Update(ObjectRegistration registration)
        {
            var result = _objects.AddOrUpdate(registration.Id, registration, (id, val) => registration);

            return result is not null;
        }

        public bool Delete(Guid objectId)
        {
            var result = _objects.TryRemove(objectId, out var _);

            return result;
        }
    }
}
