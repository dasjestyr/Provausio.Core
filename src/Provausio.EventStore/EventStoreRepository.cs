using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Provausio.Core.Logging;
using Provausio.Practices.EventSourcing;
using Provausio.Practices.EventSourcing.Deserialization;
using Provausio.Practices.Validation.Assertion;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventData = EventStore.ClientAPI.EventData;
using EventInfo = Provausio.Practices.EventSourcing.EventInfo;

namespace Provausio.EventStore
{
    public class EventStoreRepository<T> : IRepository<T>
        where T : class, IAggregate
    {
        private readonly int _maxFetchSize;
        private readonly string _streamNameFormat;
        private readonly string _streamSnapshotFormat;
        private readonly IEventDeserializationFactory _deserializationFactory;
        private readonly IEventStoreConnection _connection;

        public EventStoreRepository(
            string streamName, 
            IEventDeserializationFactory deserializationFactory,
            IEventStoreConnection connection, 
            int maxFetchSize = 1000)
        {
            _deserializationFactory = Ensure.IsNotNull(deserializationFactory, nameof(deserializationFactory));
            _connection = Ensure.IsNotNull(connection, nameof(connection));
            _streamNameFormat = $"{Ensure.IsNotNullOrEmpty(streamName, nameof(streamName))}-{{0}}";
            _streamSnapshotFormat = $"{_streamNameFormat}-snapshot";
            _maxFetchSize = maxFetchSize;
        }

        public async Task ConnectAsync()
        {
            await _connection.ConnectAsync().ConfigureAwait(false);
        }

        public void Disconnect()
        {
            _connection.Close();
        }

        public virtual Task Save(T entity)
        {
            return Save(entity, true);
        }

        public async Task Save(T entity, bool markAsCommitted)
        {
            var events = entity.GetUncommittedEvents();

            if (events.Count == 0)
                return;

            var streamName = string.Format(_streamNameFormat, entity.Id);

            await AppendEventsAsync(
                    streamName,
                    events.Cast<EventInfo>().ToArray())
                .ConfigureAwait(false);

            if(markAsCommitted)
                entity.MarkChangesAsCommitted();
        }

        private async Task AppendEventsAsync(string streamName, params EventInfo[] infos)
        {
            if (string.IsNullOrEmpty(streamName))
                throw new ArgumentNullException(nameof(streamName));

            if (infos == null || !infos.Any())
                throw new ArgumentNullException(nameof(infos));

            var first = infos[0];

            if (infos.Any(i => i.EntityId != first.EntityId))
                throw new InvalidOperationException("All events in this transaction must belong to the same entity.");

            var stream = infos
                .Select(info => new EventData(
                        info.EventId, 
                        info.EventType, 
                        true, 
                        info.GetData(), 
                        info.Metadata.GetData()))
                .ToArray();

            try
            {
                // TODO: figure out version checking
                await _connection
                    .AppendToStreamAsync(streamName, ExpectedVersion.Any, stream)
                    .ConfigureAwait(false);
            }
            catch (WrongExpectedVersionException ex)
            {
                var dirtyStreamEx = new DirtyStreamException(
                    $"Expected version {ExpectedVersion.Any} on {streamName}, but the current version was different. " +
                     "This happens when the stream was modified by some other process before this " +
                     "process was able to complete (concurrency)", ex);

                throw dirtyStreamEx;
            }
        }
        
        public virtual async Task<T> GetById(Guid id, bool useSnapshot)
        {
            var snapshotName = string.Format(_streamSnapshotFormat, id);
            
            var startPosition = 0L;
            EventInfo snapshot = null;
            if (useSnapshot)
            {
                snapshot = await GetSnapshot(snapshotName).ConfigureAwait(false);

                // if we have a snapshot, start on the next event
                // otherwise, we need everything to get a good state
                startPosition = snapshot?.Version + 1 ?? 0;
            }

            var result = await GetEvents(
                    string.Format(_streamNameFormat, id),
                    startPosition)
                .ConfigureAwait(false);

            var events = result.Events;

            if (!events.Any())
                return null;
            
            var entity = ConstructDefault();

            // snapshot must load first
            if (snapshot != null)
                events.Insert(0, snapshot);

            entity.LoadFromHistory(events);

            if (!result.NeedsSnapshot)
                return entity;

            // save a new snapshot
            var newSnapshot = entity.GetSnapshot();
            newSnapshot.Version = events.Last().Version;

            await AppendEventsAsync(snapshotName, newSnapshot)
                .ConfigureAwait(false);
            
            return entity;
        }

        private async Task<EventStreamResult> GetEvents(string streamName, long startingPosition)
        {
            if (string.IsNullOrEmpty(streamName))
                throw new ArgumentNullException(nameof(streamName));

            // collect all the slices from the starting point on
            var events = new List<EventInfo>();
            StreamEventsSlice slice;
            do
            {
                slice = await _connection
                    .ReadStreamEventsForwardAsync(
                        streamName,
                        startingPosition,
                        _maxFetchSize,
                        true)
                    .ConfigureAwait(false);

                // should only happen in tests
                if (slice == null)
                    return new EventStreamResult { Events = events };

            } while(!slice.IsEndOfStream);

            events = new List<EventInfo>();
            foreach (var @event in slice.Events)
            {
                if (!_deserializationFactory.TryDeserialize(@event.Event.Data, @event.Event.Metadata, out EventInfo info))
                    throw new InvalidOperationException($"Could not find an object to deserialize {@event.Event.EventType}");

                info.Version = @event.Event.EventNumber;

                events.Add(info);
            }

            var result = new EventStreamResult
            {
                Events = events,
                NeedsSnapshot = slice.Events.Length > _maxFetchSize
            };

            return result;
        }

        public async Task<EventInfo> GetSnapshot(string streamName)
        {
            var snapshot = await _connection
                .ReadStreamEventsBackwardAsync(
                    streamName,
                    StreamPosition.Start,
                    1,
                    true)
                .ConfigureAwait(false);

            if (snapshot?.Events == null || !snapshot.Events.Any())
                return null;

            Logger.Debug($"Found a snapshot for {streamName}", this);
            var lastEvent = snapshot.Events[0];

            if (!_deserializationFactory.TryDeserialize(lastEvent.Event.Data, lastEvent.Event.Metadata, out EventInfo info))
                throw new InvalidOperationException($"Unable to deserialize {lastEvent.Event.EventType}");

            info.Version = lastEvent.Event.EventNumber;

            return info;
        }

        public static T ConstructDefault()
        {
            var t = typeof(T);

            var ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new Type[0], null);

            if(ci == null)
                throw new InvalidOperationException($"The type ({t.FullName}) requires a public or private default constructor.");

            return (T) ci.Invoke(null);
        }
    }
}
