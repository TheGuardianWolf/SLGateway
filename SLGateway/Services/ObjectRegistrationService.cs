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
        private readonly IObjectRegistrationRepository _orr;

        public ObjectRegistrationService(ILogger<ObjectRegistrationService> logger, IObjectRegistrationRepository orr)
        {
            _logger = logger;
            _orr = orr;
        }

        public bool IsRegistered(Guid id)
        {
            return _orr.Get(id) is not null;
        }

        public ObjectRegistration GetObject(Guid id)
        {
            return _orr.Get(id);
        }

        public bool Register(ObjectRegistration reg)
        {
            return _orr.Update(reg);
        }

        public bool Deregister(Guid id)
        {
            return _orr.Delete(id);
        }
    }
}
