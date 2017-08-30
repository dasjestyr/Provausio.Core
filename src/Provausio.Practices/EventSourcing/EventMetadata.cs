using Provausio.Practices.EventSourcing;

namespace Provausio.Practices.EventSourcing
{
    public class EventMetadata : JsonEventData
    {
        public string AssemblyQualifiedType { get; set; }

        public string EventNamespaces { get; set; }
    }
}