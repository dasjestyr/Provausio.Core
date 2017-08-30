using System;
using System.Collections;
using System.Collections.Generic;

namespace Provausio.Practices.EventSourcing
{
    public interface IAggregate
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        Guid Id { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        int Version { get; }

        /// <summary>
        /// Applies the event.
        /// </summary>
        /// <param name="event">The event.</param>
        void ApplyEvent(EventInfo @event);

        /// <summary>
        /// Gets the uncommitted events.
        /// </summary>
        /// <returns></returns>
        ICollection GetUncommittedEvents();

        /// <summary>
        /// Clears the uncommitted events.
        /// </summary>
        void ClearUncommittedEvents();

        /// <summary>
        /// Marks the changes as committed.
        /// </summary>
        void MarkChangesAsCommitted();

        /// <summary>
        /// Loads from history.
        /// </summary>
        /// <param name="history">The history.</param>
        void LoadFromHistory(IEnumerable<EventInfo> history);

        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <returns></returns>
        EventInfo GetSnapshot();

        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetSnapshot<T>() where T : EventInfo;
    }
}
