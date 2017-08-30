using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Provausio.MongoDB.Repository
{
    public class MongoRepository<T> : IMongoRepository<T>
        where T : IMongoEntity
    {
        public IMongoCollection<T> DatabaseCollection { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="collectionName">Name of the collection.</param>
        public MongoRepository(MongoUrl url, string collectionName)
        {
            var mongoUrl = url;
            var client = new MongoClient(mongoUrl);

            var db = client.GetDatabase(mongoUrl.DatabaseName);
            DatabaseCollection = db.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="collectionName">Name of the collection.</param>
        public MongoRepository(string connectionString, string collectionName)
            : this(new MongoUrl(connectionString), collectionName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            DatabaseCollection = database.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// Gets the by object id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<T> GetByIdAsync(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            var result = await DatabaseCollection.FindAsync(filter).ConfigureAwait(false);
            return await result.SingleOrDefaultAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Searches the database using the provided criteria.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var query = DatabaseCollection.AsQueryable().Where(predicate);
            return await Task.Run(() => query.ToList()).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs a raw query against the database.
        /// </summary>
        /// <param name="bsonQuery">The bson query.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FindAsync(string bsonQuery)
        {
            var asBson = BsonSerializer.Deserialize<BsonDocument>(bsonQuery);
            var cursor = await DatabaseCollection.FindAsync(asBson).ConfigureAwait(false);

            var results = new List<T>();
            while (await cursor.MoveNextAsync())
            {
                results.AddRange(cursor.Current);
            }

            return results;
        }

        /// <summary>
        /// Inserts the document into the database.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public async Task<T> AddAsync(T entity)
        {
            await DatabaseCollection.InsertOneAsync(entity).ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// Inserts the documents into the database.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public async Task AddAsync(IEnumerable<T> entities)
        {
            await DatabaseCollection.InsertManyAsync(entities).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the document in the database, using the object id.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public async Task<T> UpdateAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
            await DatabaseCollection.ReplaceOneAsync(filter, entity).ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// Updates the documents in the database, using the objects ids.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public async Task UpdateAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                await UpdateAsync(entity).ConfigureAwait(false);
            }
        }
        
        /// <summary>
        /// Deletes the document from the database.
        /// </summary>
        /// <param name="objectId">The identifier.</param>
        /// <returns></returns>
        public async Task DeleteAsync(ObjectId objectId)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, objectId);
            await DatabaseCollection.DeleteOneAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the document from the database, using the object id.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public async Task DeleteAsync(T entity)
        {
            await DeleteAsync(entity.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes any document matching the provided criteria from the database.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            foreach (var entity in DatabaseCollection.AsQueryable().Where(predicate))
            {
                await DeleteAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes all documents from the database.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAllAsync()
        {
            var filter = new BsonDocument();
            await DatabaseCollection.DeleteManyAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a count of the current collection.
        /// </summary>
        /// <returns></returns>
        public async Task<long> CountAsync()
        {
            var filter = new BsonDocument();
            return await DatabaseCollection.CountAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a result count of all documents matching the provided criteria.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public async Task<long> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await DatabaseCollection.CountAsync(predicate).ConfigureAwait(false);
        }
    }
}
