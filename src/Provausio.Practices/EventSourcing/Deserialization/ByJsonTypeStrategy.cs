using System.Text;
using Newtonsoft.Json;

namespace Provausio.Practices.EventSourcing.Deserialization
{
    public class ByJsonTypeStrategy : EventDeserializationStrategy
    {
        protected override EventInfo Deserialize(byte[] eventData, byte[] eventMetadata)
        {
            return (EventInfo)JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(eventData),
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
