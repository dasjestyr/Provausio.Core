using Provausio.MongoDB.Repository;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Provausio.MongoDB
{
    public class ProjectionCacheItem : MongoEntity
    {
        [BsonRepresentation(BsonType.String)]
        public string ProjectionName { get; set; }
        
        [BsonRepresentation(BsonType.Int64)]
        public long LastSeenIndex { get; set; }
    }
}
