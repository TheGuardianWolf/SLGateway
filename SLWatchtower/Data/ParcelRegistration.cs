using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SLWatchtower.Data
{
    public class ParcelRegistration
    {
        public string ParcelName { get; set; }
        public Guid ParcelId { get; set; }
        public Guid ObjectId { get; set; }
        public string ApiKey { get; set; }
        public string UserId { get; set; }
    }

    public class ParcelRegistrationEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string ParcelName { get; set; }
        public Guid ParcelId { get; set; }
        public Guid ObjectId { get; set; }
        public string ApiKey { get; set; }
        public string UserId { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public static class ParcelRegistrationExtensions
    {
        public static ParcelRegistration ToApiKey(this ParcelRegistrationEntity entity)
        {
            return new ParcelRegistration
            {
                ApiKey = entity.ApiKey,
                UserId = entity.UserId,
                ObjectId = entity.ObjectId,
                ParcelId = entity.ParcelId,
                ParcelName = entity.ParcelName,
            };
        }

        public static ParcelRegistrationEntity ToEntity(this ParcelRegistration apiKey)
        {
            return new ParcelRegistrationEntity
            {
                ApiKey = apiKey.ApiKey,
                UserId = apiKey.UserId,
                ParcelName = apiKey.ParcelName,
                ParcelId = apiKey.ParcelId,
                ObjectId = apiKey.ObjectId,
                LastModifiedDate = DateTime.UtcNow
            };
        }
    }
}
