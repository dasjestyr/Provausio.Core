using System;
using System.Linq;
using Provausio.Core.Ext;
using Provausio.Practices.Validation.Assertion;

namespace Provausio.Practices.EventSourcing
{
    public abstract class EventInfo : JsonEventData
    {
        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public string EventType => GetType().Name;

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        public EventMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the event identifier. Used for idempotent operations.
        /// </summary>
        /// <value>
        /// The event identifier.
        /// </value>
        public Guid EventId { get; set; }

        /// <summary>
        /// Gets or sets the version (the event sequence number).
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }

        /// <summary>
        /// Gets or sets the reason for the event.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public string Reason { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventInfo"/> class.
        /// </summary>
        protected EventInfo()
        {
            EventId = Guid.NewGuid();

            var nameSpaces = this.FindAttributes<EventNamespaceAttribute>();
            var namespaceNames = string.Join(";", nameSpaces.Select(ns => ns.Namespace));

            Metadata = new EventMetadata
            {
                AssemblyQualifiedType = GetType().AssemblyQualifiedName,
                EventNamespaces = namespaceNames
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventInfo" /> class.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reason">The reason.</param>
        protected EventInfo(Guid entityId, string reason) 
            : this()
        {
            EntityId = Ensure.IsNotDefault(entityId, nameof(entityId));
            Reason = Ensure.IsNotNullOrEmpty(reason, nameof(reason));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EventInfo);
        }

        protected bool Equals(EventInfo other)
        {
            return other != null && EventId.Equals(other.EventId);
        }

        public override int GetHashCode()
        {
            // needed to allow setter for deserialization :/
            return EventId.GetHashCode();
        }
    }
}
