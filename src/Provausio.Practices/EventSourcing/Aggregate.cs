using System;
using System.Collections;
using System.Collections.Generic;

namespace Provausio.Practices.EventSourcing
{
    public abstract class Aggregate<TSnapshotType> : IAggregate
        where TSnapshotType : EventInfo
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

        protected abstract TSnapshotType BuildSnapshot();

        /// <summary>
        /// Returns and instance of <see cref="EventInfo"/> representing the state of the aggregate.
        /// </summary>
        /// <returns></returns>
        public EventInfo GetSnapshot()
        {
            return BuildSnapshot();
        }

        /// <summary>
        /// Returns the state of the aggregate (the snapshot)
        /// </summary>
        /// <returns></returns>
        public TSnapshotType GetState()
        {
            return (TSnapshotType)GetSnapshot();
        }

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
                Apply((dynamic)e, false);
        }

        /// <summary>
        /// Applies the event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void ApplyEvent(EventInfo @event)
        {
            @event.EntityId = Id;
            Apply(@event, true);
        }

        private void Apply(EventInfo @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);

            if (isNew)
                Changes.Add(@event);
        }
    }
}
