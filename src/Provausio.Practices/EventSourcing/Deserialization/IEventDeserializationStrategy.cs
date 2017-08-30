namespace Provausio.Practices.EventSourcing.Deserialization
{
    public interface IEventDeserializationStrategy
    {
        EventInfo DeserializeEvent(byte[] eventData, byte[] eventMetaData);
    }
}