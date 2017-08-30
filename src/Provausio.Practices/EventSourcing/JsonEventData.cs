using System.Text;
using Newtonsoft.Json;

namespace Provausio.Practices.EventSourcing
{
    public class JsonEventData
    {
        public byte[] GetData()
        {
            var asJson = JsonConvert.SerializeObject(this, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            return Encoding.UTF8.GetBytes(asJson);
        }
    }
}