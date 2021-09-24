using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Data
{
    public class ObjectRegistration
    {
        public Guid ObjectId { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public string ApiKey { get; set; }
        public string UserId { get; set; }
    }

    public class ObjectRegistrationEntity
    {
        [BsonId]
        public string Id { get; set; }
        public Guid ObjectId { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public string ApiKey { get; set; }
        public string UserId { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public static class ObjectRegistrationExtensions
    {
        public static ObjectRegistration ToObjectRegistration(this ObjectRegistrationEntity entity)
        {
            return new ObjectRegistration
            {
                ApiKey = entity.ApiKey,
                ObjectId = entity.ObjectId,
                Url = entity.Url,
                Token = entity.Token,
                UserId = entity.UserId,
            };
        }

        public static ObjectRegistrationEntity ToEntity(this ObjectRegistration objectRegistration)
        {
            return new ObjectRegistrationEntity
            {
                ApiKey = objectRegistration.ApiKey,
                ObjectId = objectRegistration.ObjectId,
                Url = objectRegistration.Url,
                Token = objectRegistration.UserId,
                UserId = objectRegistration.UserId,
                LastModifiedDate = DateTime.UtcNow
            };
        }
    }
}
