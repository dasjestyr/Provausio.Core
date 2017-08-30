using System;
using System.Text;
using Newtonsoft.Json;

namespace Provausio.Practices.EventSourcing.Deserialization
{
    public class ByAssemblyQualifiedTypeNameStrategy : EventDeserializationStrategy
    {
        protected override EventInfo Deserialize(byte[] eventData, byte[] eventMetadata)
        {
            var metaData = JsonConvert.DeserializeObject<EventMetadata>(Encoding.UTF8.GetString(eventMetadata));
            if (string.IsNullOrEmpty(metaData?.AssemblyQualifiedType))
                throw new InvalidOperationException("Invalid or missing event metadata.");

            var type = Type.GetType(metaData.AssemblyQualifiedType);
            return (EventInfo)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(eventData), type);
        }
    }
}