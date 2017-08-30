using System;
using Provausio.Core.Ext;
using Provausio.PracticesEventSourcing;

namespace Provausio.Practices.EventSourcing.Deserialization
{
    /// <summary>
    /// Will use attribute to match on AssemblyQualifiedType found in the event metadata
    /// </summary>
    /// <seealso cref="ByAttributeStrategy" />
    public class AttributeAsAssemblyQualifiedNameStrategy : ByAttributeStrategy
    {
        public AttributeAsAssemblyQualifiedNameStrategy(params Type[] typeRefs)
            : base(typeRefs) { }

        public AttributeAsAssemblyQualifiedNameStrategy(string path)
            : base(path) { }

        protected override EventInfo Deserialize(byte[] eventData, byte[] eventMetadata)
        {
            var metaData = eventMetadata.DeserializeJson<EventMetadata>();
            var eventNamespace = new EventNamespace(metaData.AssemblyQualifiedType);

            // use simplified assembly name to ignore revision/build
            var simpleNamespace = eventNamespace.GetSimpleNamespace();
            var dtoType = FindType(simpleNamespace);
            var deserialized = eventData.DeserializeJson<EventInfo>(dtoType);
            return deserialized;
        }
    }
}