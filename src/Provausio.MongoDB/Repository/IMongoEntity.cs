using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Provausio.MongoDB.Repository
{
    public interface IMongoEntity : IMongoEntity<ObjectId> { }

    public interface IMongoEntity<T>
    {
        /// <summary>
        /// Gets or sets the mongo db ID
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [BsonId]
        T Id { get; set; }
    }
}