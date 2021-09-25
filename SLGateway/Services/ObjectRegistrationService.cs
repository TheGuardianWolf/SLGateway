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
        Task<bool> Deregister(Guid id);
        Task<ObjectRegistration> GetObject(Guid id);
        Task<bool> IsRegistered(Guid id);
        Task<bool> Register(ObjectRegistration reg);
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

        public async Task<bool> IsRegistered(Guid id)
        {
            return await _objectRegistrationRepository.Get(id) is not null;
        }

        public async Task<ObjectRegistration> GetObject(Guid id)
        {
            return await _objectRegistrationRepository.Get(id);
        }

        public async Task<bool> Register(ObjectRegistration reg)
        {
            return await _objectRegistrationRepository.Update(reg);
        }

        public async Task<bool> Deregister(Guid id)
        {
            var obj = await _objectRegistrationRepository.Get(id);
            if (obj is null)
            {
                return false;
            }

            var deregisterResult = await _objectRegistrationRepository.Delete(id);

            return deregisterResult;
        }
    }
}
