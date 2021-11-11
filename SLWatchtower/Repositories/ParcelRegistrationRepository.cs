using MongoDB.Driver;
using SLWatchtower.Data;

namespace SLWatchtower.Repositories
{
    public interface IParcelRegistrationRepository
    {
        Task<IEnumerable<ParcelRegistrationEntity>> GetByUser(string userId);
        Task<bool> Update(ParcelRegistrationEntity entity);
        Task<bool> Delete(string userId, Guid objectId);
    }

    public class ParcelRegistrationRepository : IParcelRegistrationRepository
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<ParcelRegistrationEntity> _collection;

        public ParcelRegistrationRepository(ILogger<ParcelRegistrationRepository> logger, IMongoDataSource mongoDataSource)
        {
            _logger = logger;
            _collection = mongoDataSource.Database.GetCollection<ParcelRegistrationEntity>("ParcelRegistration");
        }

        public async Task<IEnumerable<ParcelRegistrationEntity>> GetByUser(string userId)
        {
            var cursor = await _collection.FindAsync(x => x.UserId == userId);
            return await cursor.ToListAsync();
        }

        public async Task<bool> Delete(string userId, Guid objectId)
		{
            var result = await _collection.DeleteOneAsync(x => x.UserId == userId && x.ObjectId == objectId);
            return result.IsAcknowledged;
		}

        public async Task<bool> Update(ParcelRegistrationEntity entity)
        {
            var result = await _collection.ReplaceOneAsync(c => c.UserId == entity.UserId && c.ObjectId == entity.ObjectId, entity, new ReplaceOptions
            {
                IsUpsert = true
            });

            return result is not null;
        }
    }
}
