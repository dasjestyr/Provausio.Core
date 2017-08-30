using System;

namespace Provausio.Practices.EventSourcing
{
    public interface ISubscriptionAdapter
    {
        bool EnableSubscriptionLogging { get; set; }

        /// <summary>
        /// Starts a catch-up subscription.
        /// </summary>
        /// <param name="streamName">Name of the stream.</param>
        /// <param name="lastSeenIndex">Last index of the stream.</param>
        /// <param name="processAction">Delegate that will process an event.</param>
        /// <param name="liveQueueSize">The size of the queue when running live subscription.</param>
        /// <param name="readBatchSize">The number of events that will be processed at a time when catchup is running.</param>
        /// <param name="enableVerboseClientLogging"></param>
        /// <param name="resolveLinkTos">Whether or not a link-to should be resolved.</param>
        void CatchUpSubscription(
            string streamName, 
            long lastSeenIndex, 
            Action<EventInfo, long> processAction,
            int liveQueueSize = 10000,
            int readBatchSize = 500,
            bool enableVerboseClientLogging = false,
            bool resolveLinkTos = true);

        /// <summary>
        /// Starts a persistent subscription.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="streamName">Name of the stream.</param>
        /// <param name="processAction">Delegate that will process an event.</param>
        void PersistentSubscription(
            string groupName,
            string streamName,
            Action<EventInfo, long> processAction);

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
