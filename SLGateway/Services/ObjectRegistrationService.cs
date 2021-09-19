using Microsoft.Extensions.Logging;
using SLGateway.Data;
using SLGateway.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Services
{
    public interface IObjectRegistrationService
    {
        public bool IsRegistered(Guid id);
        ObjectRegistration GetObject(Guid id);
        bool Register(ObjectRegistration reg);
        bool Deregister(Guid id);
    }

    public class ObjectRegistrationService : IObjectRegistrationService
    {
        private readonly ILogger _logger;
        private readonly IObjectRegistrationRepository _objectRegistrationRepository;

        public ObjectRegistrationService(ILogger<ObjectRegistrationService> logger, IObjectRegistrationRepository objectRegistrationRepository)
        {
            _logger = logger;
            _objectRegistrationRepository = objectRegistrationRepository;
        }

        public bool IsRegistered(Guid id)
        {
            return _objectRegistrationRepository.Get(id) is not null;
        }

        public ObjectRegistration GetObject(Guid id)
        {
            return _objectRegistrationRepository.Get(id);
        }

        public bool Register(ObjectRegistration reg)
        {
            return _objectRegistrationRepository.Update(reg);
        }

        public bool Deregister(Guid id)
        {
            return _objectRegistrationRepository.Delete(id);
        }
    }
}
