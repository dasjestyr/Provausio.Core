using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Provausio.MongoDB.Repository
{
    public interface IMongoRepository<T>
        where T : IMongoEntity
    {
        /// <summary>
        /// Exposes the internal collection reference to allow for more explicity querying.
        /// </summary>
        IMongoCollection<T> DatabaseCollection { get; }
            /// <summary>
        /// Gets the by object id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<T> GetByIdAsync(ObjectId id);

        /// <summary>
        /// Searches the database using the provided criteria.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Runs a raw query against the database.
        /// </summary>
        /// <param name="bsonQuery">The bson query.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync(string bsonQuery);

        /// <summary>
        /// Inserts the document into the database.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Inserts the documents into the database.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        Task AddAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates the document in the database, using the object id.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Updates the documents in the database, using the objects ids.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        Task UpdateAsync(IEnumerable<T> entities);

        /// <summary>
        /// Deletes the document from the database.
        /// </summary>
        /// <param name="objectId">The identifier.</param>
        /// <returns></returns>
        Task DeleteAsync(ObjectId objectId);

        /// <summary>
        /// Deletes the document from the database, using the object id.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Deletes any document matching the provided criteria from the database.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        Task DeleteAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Deletes all documents from the database.
        /// </summary>
        /// <returns></returns>
        Task DeleteAllAsync();

        /// <summary>
        /// Returns a count of the current collection.
        /// </summary>
        /// <returns></returns>
        Task<long> CountAsync();

        /// <summary>
        /// Returns a result count of all documents matching the provided criteria.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        Task<long> CountAsync(Expression<Func<T, bool>> predicate);
    }
}