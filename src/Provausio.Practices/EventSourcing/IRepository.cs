using System;
using System.Threading.Tasks;

namespace Provausio.Practices.EventSourcing
{
    public interface IRepository<T>
        where T : IAggregate
    {
        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task Save(T entity);

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="useSnapshot">if set to <c>true</c> [use snapshot].</param>
        /// <returns></returns>
        Task<T> GetById(Guid id, bool useSnapshot);
    }
}
