using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAS.ElasticSearch;
using DAS.ElasticSearch.Ext;
using Elasticsearch.Net;
using Nest;
using Provausio.Core.Logging;
using Provausio.EventStore;
using Provausio.Practices.EventSourcing;
using Provausio.Practices.Validation.Assertion;

namespace Provausio.ElasticSearch
{
    public class EventStoreElasticSearchProjection : EventStoreProjection<IElasticClient>
    {
        private const string CacheTypeName = "index_cache";

        private readonly string _indexCacheId;
        private readonly IElasticClient _elasticClient;

        protected bool ThrowOnInvalidResponse { get; set; }

        protected EventStoreElasticSearchProjection(
            string streamName,
            string projectionName,
            ISubscriptionAdapter subscriptionAdapter,
            IElasticClient elasticClient)
            : base(projectionName, streamName, subscriptionAdapter, elasticClient)
        {
            _elasticClient = Ensure.IsNotNull(elasticClient, nameof(elasticClient));
            _indexCacheId = $"{Name}_lastIndex";
        }

        protected override async Task<long> GetLastIndexAsync()
        {
            var indexCacheRequest = new GetRequest<ProjectionCacheItem>(
                Name,
                CacheTypeName,
                _indexCacheId);

            var indexResponse = await _elasticClient
                .GetAsync<ProjectionCacheItem>(indexCacheRequest)
                .ConfigureAwait(false);

            long lastIndex;
            if (indexResponse.Found)
            {
                lastIndex = indexResponse.Source.LastKnownIndex;
            }
            else
            {
                lastIndex = StartFromBeginning;
                await UpdateIndexAsync(StartFromBeginning);
            }

            Logger.Verbose($"({GetType().FullName}) last recorded index was {lastIndex}...", this);
            return lastIndex;
        }

        protected override async Task UpdateIndexAsync(long latestIndex)
        {
            // if the record doesn't exist, create it
            // if the record does exist update it, but only if the new index is greater than the existing index

            var request = new UpdateRequest<ProjectionCacheItem, ProjectionCacheItem>(
                Name, CacheTypeName, _indexCacheId)
            {
                Script = new InlineScript(
                    "if(ctx._source.lastKnownIndex >= params.lastKnownIndex) { ctx.op = 'noop' }" +
                    "ctx._source.lastKnownIndex=params.lastKnownIndex")
                {
                    Params = new Dictionary<string, object>
                    {
                        {"projectionName", Name},
                        {"lastKnownIndex", latestIndex}
                    },
                    Lang = "painless"
                },
                Upsert = new ProjectionCacheItem // keep in mind that these properties get serialized as camelCase
                {
                    ProjectionName = Name,
                    LastKnownIndex = latestIndex
                },
                Refresh = Refresh.True
            };

            try
            {
                var response = await _elasticClient
                    .UpdateAsync<ProjectionCacheItem>(request)
                    .ConfigureAwait(false);

                if (response.IsValid)
                {
                    Logger.Verbose("ElasticSearch index cache was updated. {lastSeenIndex}", this, latestIndex);
                }
                else
                {
                    if(ThrowOnInvalidResponse)
                    {
                        Logger.Fatal("Unable to update index cache. New index = {newIndex} {debugInfo} {request}", this,
                        response.OriginalException,
                        latestIndex,
                        response.DebugInformation,
                        request.GetJson(_elasticClient));

                        throw new Exception($"Failed to update index {latestIndex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("Failed to update ElasticSearch index cache: {message}", this, ex, ex.Message);
            }
        }
    }
}
