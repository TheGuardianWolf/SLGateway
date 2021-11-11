using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SLWatchtower.Data
{
    public class ParcelRegistration
    {
        public string ParcelName { get; set; }
        public string ParcelDescription { get; set; }
        public Guid ParcelId { get; set; }
        public Guid ObjectId { get; set; }
        public string UserId { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class ParcelRegistrationEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id { get; set; }
        public string ParcelName { get; set; }
        public string ParcelDescription { get; set; }
        public Guid ParcelId { get; set; }
        public Guid ObjectId { get; set; }
        public string UserId { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public static class ParcelRegistrationExtensions
    {
        public static ParcelRegistration ToParcelRegistration(this ParcelRegistrationEntity entity)
        {
            return new ParcelRegistration
            {
                UserId = entity.UserId,
                ObjectId = entity.ObjectId,
                ParcelId = entity.ParcelId,
                ParcelName = entity.ParcelName,
                ParcelDescription = entity.ParcelDescription,
                LastModifiedDate = entity.LastModifiedDate,
            };
        }

        public static ParcelRegistrationEntity ToEntity(this ParcelRegistration reg)
        {
            return new ParcelRegistrationEntity
            {
                UserId = reg.UserId,
                ParcelName = reg.ParcelName,
                ParcelDescription = reg.ParcelDescription,
                ParcelId = reg.ParcelId,
                ObjectId = reg.ObjectId,
                LastModifiedDate = DateTime.UtcNow
            };
        }
    }
}
