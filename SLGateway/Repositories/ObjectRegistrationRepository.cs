using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
        Task<bool> Delete(Guid objectId);
        Task<ObjectRegistration> Get(Guid objectId);
        Task<bool> Update(ObjectRegistration registration);
    }

    public class ObjectRegistrationRepository : IObjectRegistrationRepository
    {
        private IMongoCollection<ObjectRegistrationEntity> _collection;
        private ILogger _logger;

        public ObjectRegistrationRepository(ILogger<ObjectRegistrationRepository> logger, IMongoDataSource mongoDataSource)
        {
            _logger = logger;
            _collection = mongoDataSource.Database.GetCollection<ObjectRegistrationEntity>("ObjectRegistrations");
        }

        public async Task<ObjectRegistration> Get(Guid objectId)
        {
            var cursor = await _collection.FindAsync(c => c.ObjectId == objectId);
            var entity = cursor.FirstOrDefault();
            if (entity is null)
            {
                return null;
            }

            return entity.ToObjectRegistration();
        }

        public async Task<bool> Update(ObjectRegistration registration)
        {
            var result = await _collection.UpdateOneAsync(c => c.ObjectId == registration.ObjectId, null, new UpdateOptions
            {
                IsUpsert = true
            });

            return result is not null;
        }

        public async Task<bool> Delete(Guid objectId)
        {
            var result = await _collection.DeleteOneAsync(c => c.ObjectId == objectId);

            return result is not null;
        }
    }
}
