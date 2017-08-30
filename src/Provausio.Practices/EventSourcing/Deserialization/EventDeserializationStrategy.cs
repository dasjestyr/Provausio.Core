using Provausio.Practices.Validation.Assertion;

namespace Provausio.Practices.EventSourcing.Deserialization
{
    public abstract class EventDeserializationStrategy : IEventDeserializationStrategy
    {
        protected abstract EventInfo Deserialize(byte[] eventData, byte[] eventMetadata);

        public EventInfo DeserializeEvent(byte[] eventData, byte[] eventMetaData)
        {
            Validate(eventData, eventMetaData);
            return Deserialize(eventData, eventMetaData);
        }

        protected void Validate(byte[] eventData, byte[] metaData)
        {
            Ensure.IsNotNull(eventData, nameof(eventData));
            Ensure.IsNotNull(metaData, nameof(metaData));
        }
    }
}