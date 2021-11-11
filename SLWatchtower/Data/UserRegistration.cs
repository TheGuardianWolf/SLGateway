using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SLWatchtower.Data
{
    public class UserRegistration
    {
        public string UserId { get; set; }
        public string ApiKey { get; set; }
    }

    public class UserRegistrationEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id {  get; set; }
        public string UserId { get; set; }
        public string ApiKey { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public static class RegistrationExtenstions
    {
        public static UserRegistration ToUserRegistration(this UserRegistrationEntity entity)
        {
            return new UserRegistration
            {
                ApiKey = entity.ApiKey,
                UserId = entity.UserId
            };
        }

        public static UserRegistrationEntity ToEntity(this UserRegistration reg)
        {
            return new UserRegistrationEntity
            {
                ApiKey = reg.ApiKey,
                UserId = reg.UserId,
                LastModifiedDate = DateTime.UtcNow
            };
        }
    }
}
