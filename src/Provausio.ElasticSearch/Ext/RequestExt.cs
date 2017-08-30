using System.IO;
using System.Text;
using Nest;

namespace DAS.ElasticSearch.Ext
{
    public static class RequestExt
    {
        public static string GetJson(this IRequest request, IElasticClient client)
        {
            var s = new MemoryStream();
            client.Serializer.Serialize(request, s);
            var json = Encoding.UTF8.GetString(s.ToArray());
            return json;
        }
    }
}
