using Newtonsoft.Json;
using System.Net.Http;

namespace Provausio.Core.Ext
{
    public static class HttpWebResponseExt
    {
        public static T Deserialize<T>(this HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
        }
    }
}
