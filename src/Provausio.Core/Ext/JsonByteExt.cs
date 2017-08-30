using System;
using System.Text;
using Newtonsoft.Json;

namespace Provausio.Core.Ext
{
    public static class JsonByteExt
    {
        public static T DeserializeJson<T>(this byte[] data)
        {
            var asString = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(asString);
        }

        public static T DeserializeJson<T>(this byte[] data, Type type)
        {
            var asString = Encoding.UTF8.GetString(data);
            return (T) JsonConvert.DeserializeObject(asString, type);
        }

        public static T DeserializeJson<T>(this byte[] data, JsonSerializerSettings serializerSettings)
        {
            var asString = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(asString, serializerSettings);
        }

        public static T DeserializeJson<T>(this byte[] data, params JsonConverter[] converters)
        {
            var asString = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(asString, converters);
        }
    }
}
