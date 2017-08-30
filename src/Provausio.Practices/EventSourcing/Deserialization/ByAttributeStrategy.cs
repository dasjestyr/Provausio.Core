using System;
using Provausio.Core.Ext;
using Provausio.Practices.Validation.Assertion;

namespace Provausio.Practices.EventSourcing.Deserialization
{
    public class ByAttributeStrategy : EventDeserializationStrategy
    {
        private readonly EventTypeCache _eventTypeCache;

        public ByAttributeStrategy(string path)
        {
            _eventTypeCache = EventTypeCache.GetInstance(path);
        }

        public ByAttributeStrategy(params Type[] typeRefs)
        {
            _eventTypeCache = EventTypeCache.GetInstance(typeRefs);
        }

        protected override EventInfo Deserialize(byte[] eventData, byte[] eventMetaData)
        {
            var metaData = eventMetaData.DeserializeJson<EventMetadata>();
            var dtoType = FindType(metaData.EventNamespaces);
            
            var deserialized = eventData.DeserializeJson<EventInfo>(dtoType);
            return deserialized;
        }

        protected Type FindType(string nameSpace)
        {
            Ensure.IsNotNullOrEmpty(nameSpace, nameof(nameSpace));
            return _eventTypeCache.ResolveType(nameSpace);
        }
    }
}