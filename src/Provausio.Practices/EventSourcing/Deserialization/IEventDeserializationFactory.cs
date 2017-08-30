using Provausio.Practices.EventSourcing;

namespace Provausio.PracticesEventSourcing.Deserialization
{
    public interface IEventDeserializationFactory
    {
        bool TryDeserialize(byte[] eventData, byte[] eventMetadata, out EventInfo e);
    }
}