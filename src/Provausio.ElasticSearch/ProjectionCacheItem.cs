namespace DAS.ElasticSearch
{
    public class ProjectionCacheItem
    {
        public string Id => ProjectionName;

        public string ProjectionName { get; set; }

        public long LastKnownIndex { get; set; }
    }
}