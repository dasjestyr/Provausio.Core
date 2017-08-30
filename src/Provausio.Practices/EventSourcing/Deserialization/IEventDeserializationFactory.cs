namespace Provausio.Practices.EventSourcing.Deserialization
{
    public interface IEventDeserializationFactory
    {
        bool TryDeserialize(byte[] eventData, byte[] eventMetadata, out EventInfo e);
    }
}