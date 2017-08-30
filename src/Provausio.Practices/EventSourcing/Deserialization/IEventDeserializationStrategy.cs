using Provausio.Practices.EventSourcing;

namespace Provausio.PracticesEventSourcing.Deserialization
{
    public interface IEventDeserializationStrategy
    {
        EventInfo DeserializeEvent(byte[] eventData, byte[] eventMetaData);
    }
}