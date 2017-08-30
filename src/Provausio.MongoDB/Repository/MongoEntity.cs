using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Provausio.MongoDB.Repository
{
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class MongoEntity : IMongoEntity
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        protected MongoEntity()
        {
            Id = ObjectId.GenerateNewId();
        }
    }
}