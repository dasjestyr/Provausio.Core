using System.Collections.Generic;

namespace Provausio.Practices.EventSourcing
{
    public class EventStreamResult
    {
        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        public List<EventInfo> Events { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not a snapshot should be taken as a followup action.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [needs snapshot]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsSnapshot { get; set; }
    }
}