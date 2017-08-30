using System;
using System.Text;
using Provausio.Core;
using Provausio.Core.Logging;
using Provausio.Practices.EventSourcing;
using Provausio.Practices.EventSourcing.Deserialization;
using EventStore.ClientAPI;

namespace Provausio.EventStore
{
    public class EventStoreSubscriptionAdapter : ISubscriptionAdapter
    {
        private Action<EventInfo, long> _processAction;
        private readonly IEventDeserializationFactory _deserializationFactory;
        private readonly IEventStoreConnection _eventStore;
        private readonly DisposableActionCollection _disposableActionCollection;

        /// <summary>
        /// If true, event store subscription events will be logged to verbose channels.
        /// </summary>
        public bool EnableSubscriptionLogging { get; set; } = false;

        public EventStoreSubscriptionAdapter(
            IEventDeserializationFactory deserializationFactory, 
            IEventStoreConnection eventStore)
        {
            _disposableActionCollection = new DisposableActionCollection();
            _deserializationFactory = deserializationFactory;
            _eventStore = eventStore;
        }

        public void CatchUpSubscription(
            string streamName,
            long lastSeenIndex,
            Action<EventInfo, long> processAction,
            int liveQueueSize = 10000,
            int readBatchSize = 500,
            bool enableVerboseClientLogging = false,
            bool resolveLinkTos = true)
        {
            _processAction = processAction;

            long? startIndex = lastSeenIndex;
            if (lastSeenIndex == -2)
                startIndex = null; // weird inconsistency in the ClientAPI
            
            Verbose($"Subscribing to {streamName} as catch-up subscription at index {lastSeenIndex}...");

            var settings = new CatchUpSubscriptionSettings(liveQueueSize, readBatchSize, enableVerboseClientLogging, resolveLinkTos);
            var sub = _eventStore.SubscribeToStreamFrom(
                streamName,
                startIndex,
                settings,
                EventAppeared, 
                subscriptionDropped: (subscription, reason, ex) => Logger.Fatal($"Subscription dropped: {reason}. {ex.Message}", this, ex));
            
            _disposableActionCollection.Add(() => sub.Stop());
        }

        public void PersistentSubscription(
            string groupName,
            string streamName,
            Action<EventInfo, long> processAction)
        {
            _processAction = processAction;
            
            Verbose($"Subscribing to {streamName} as persistent subscription...");

            var sub = _eventStore.ConnectToPersistentSubscription(
                streamName,
                groupName,
                EventAppeared,
                (subscription, reason, ex) => Logger.Fatal($"Subscription dropped: {reason}. {ex.Message}", this, ex));

            _disposableActionCollection.Add(() => sub.Stop(TimeSpan.FromSeconds(30)));
        }

        public void Stop()
        {
            _disposableActionCollection.Dispose();
        }

        private void EventAppeared(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, ResolvedEvent resolvedEvent)
        {
            EventAppeared(resolvedEvent);
        }

        private void EventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent)
        {
            EventAppeared(resolvedEvent);
        }

        private void EventAppeared(ResolvedEvent resolvedEvent)
        {
            if (_processAction == null)
                throw new InvalidOperationException("The process action was not set.");
            
            try
            {
                if (resolvedEvent.Event == null)
                {
                    Logger.Error("There was no event found on the resolved event object. Skipping...", this);
                    return;
                }

                // skip system events
                if (resolvedEvent.Event.EventType.StartsWith("$"))
                    return;
                
                Verbose($"EVENT:: {resolvedEvent.Event.EventType} ({resolvedEvent.Event.EventStreamId}; Index {resolvedEvent.OriginalPosition})");

                if (resolvedEvent.Event.EventType == "$metadata")
                {
                    Verbose($"Event is metadata and is likely a soft deleted event ({resolvedEvent.Event.EventStreamId}). Ignoring...");
                    return;
                }

                // if deserialization succeeds
                if (_deserializationFactory.TryDeserialize(resolvedEvent.Event.Data, resolvedEvent.Event.Metadata, out EventInfo info))
                {
                    Process(info, resolvedEvent.OriginalEventNumber);
                    return;
                }

                var asJson = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                Logger.Error("Could not deserialize event {@Event}", this, new
                {
                    resolvedEvent.Event.EventType,
                    EventData = asJson
                });
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Could not deserialize event. {ex.Message}", this, ex);
            }
        }

        private void Process(EventInfo e, long index)
        {
            _processAction(e, index);
        }

        private void Verbose(string pattern, params object[] parameters)
        {
            if (!EnableSubscriptionLogging)
                return;

            Logger.Verbose(pattern, this, parameters);
        }
    }
}
