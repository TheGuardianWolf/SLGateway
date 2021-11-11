using MongoDB.Driver;
using SLWatchtower.Data;

namespace SLWatchtower.Repositories
{
    public interface IUserRegistrationRepository
    {
        Task<UserRegistrationEntity> Get(string userId);
        Task<bool> Update(UserRegistrationEntity entity);
    }

    public class UserRegistrationRepository : IUserRegistrationRepository
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<UserRegistrationEntity> _collection;

        public UserRegistrationRepository(ILogger<UserRegistrationRepository> logger, IMongoDataSource mongoDataSource)
        {
            _logger = logger;
            _collection = mongoDataSource.Database.GetCollection<UserRegistrationEntity>("UserRegistration");
        }

        public async Task<UserRegistrationEntity> Get(string userId)
        {
            var cursor = await _collection.FindAsync(x => x.UserId == userId);
            return cursor.FirstOrDefault();
        }

        public async Task<bool> Update(UserRegistrationEntity entity)
        {
            var result = await _collection.ReplaceOneAsync(c => c.UserId == entity.UserId, entity, new ReplaceOptions
            {
                IsUpsert = true
            });

            return result is not null;
        }
    }
}
