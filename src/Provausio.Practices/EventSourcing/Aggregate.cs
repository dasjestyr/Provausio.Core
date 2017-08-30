using System;
using System.Collections;
using System.Collections.Generic;

namespace Provausio.Practices.EventSourcing
{
    public abstract class Aggregate : IAggregate
    {
        protected List<EventInfo> Changes = new List<EventInfo>();

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public int Version { get; protected set; }

        /// <summary>
        /// Gets the uncommitted events.
        /// </summary>
        /// <returns></returns>
        public virtual ICollection GetUncommittedEvents()
        {
            return Changes;
        }

        /// <summary>
        /// Clears the uncommitted events.
        /// </summary>
        public virtual void ClearUncommittedEvents()
        {
            Changes.Clear();
        }

        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <returns></returns>
        public EventInfo GetSnapshot()
        {
            var snapshot = BuildSnapshot();
            snapshot.EntityId = Id;
            return snapshot;
        }

        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSnapshot<T>() 
            where T : EventInfo
        {
            // deliberately use unsafe cast
            return (T) GetSnapshot();
        }

        protected abstract EventInfo BuildSnapshot();

        /// <summary>
        /// Marks the changes as committed.
        /// </summary>
        public virtual void MarkChangesAsCommitted()
        {
            ClearUncommittedEvents();
        }

        /// <summary>
        /// Loads from history.
        /// </summary>
        /// <param name="history">The history.</param>
        public void LoadFromHistory(IEnumerable<EventInfo> history)
        {
            foreach (var e in history)
                ApplyEvent((dynamic)e, false);
        }

        /// <summary>
        /// Applies the event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void ApplyEvent(EventInfo @event)
        {
            @event.EntityId = Id;
            ApplyEvent(@event, true);
        }

        private void ApplyEvent(EventInfo @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);

            if (isNew)
                Changes.Add(@event);
        }
    }
}
