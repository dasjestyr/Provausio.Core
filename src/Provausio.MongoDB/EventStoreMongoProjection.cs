using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using MongoDB.Driver;
using Polly;
using Provausio.Core.Logging;
using Provausio.EventStore;
using Provausio.Practices.EventSourcing;

namespace Provausio.MongoDB
{
    public abstract class EventStoreMongoProjection : EventStoreProjection<IMongoDatabase>
    {
        protected readonly IMongoCollection<ProjectionCacheItem> IndexCacheCollection;
        
        protected int RetryIntervalMs { get; set; } = 250;

        protected int MaxAttempts { get; set; } = 5;

        protected TimeSpan SaveActionTimeout = TimeSpan.FromSeconds(30);

        protected Func<int, TimeSpan> RetryInterval { get; set; }

        protected EventStoreMongoProjection(
            string streamName,
            string projectionName,
            ISubscriptionAdapter subscriptionAdapter,
            IMongoDatabase projectionStore) 
                : base(projectionName, streamName, subscriptionAdapter, projectionStore)
        {
            IndexCacheCollection = projectionStore.GetCollection<ProjectionCacheItem>(IndexCacheName);
        }

        protected override async Task OnSaveAsync(
            Func<IMongoDatabase, Task> saveAction, 
            long latestIndex, 
            IMongoDatabase client)
        {
            /****************************************** NOTE *****************************************
            * There are a number of ridiculous errors with the MongoDB C# driver that result
            * in the following exceptions that have been on their Jira board for quite some time. 
            * MongoDB's recommended solutions have been to retry. Here are links to some of the
            * known issues:
            * 
            * Socket exception (Socket and IOException) https://jira.mongodb.org/browse/CSHARP-1303
            * Duplicate key error (Write and BulkWriteException) https://jira.mongodb.org/browse/SERVER-14322
            *****************************************************************************************/
            
            var result = await Policy
                .Handle<MongoWriteException>()
                .Or<MongoBulkWriteException>()
                .Or<MongoConnectionException>()
                .Or<IOException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(
                    MaxAttempts,
                    // back off algorithm increases wait time between attempts with each subsequent attempt
                    RetryInterval ?? (retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                    (exception, timeSpan, retryCount, ctx) =>
                    {
                        Logger.Warning("{message}. Retrying...", this, exception.Message);
                        Logger.Verbose($"{ClassName}::Index {{index}} retry {{retryCount}}", this, latestIndex, retryCount);
                    })
                .ExecuteAndCaptureAsync(() => saveAction(client))
                .ConfigureAwait(false);

            if (result.Outcome == OutcomeType.Failure)
            {
                Logger.Fatal($"Save action (Index {latestIndex}) failed (giving up): {result.FinalException.Message}", this, result.FinalException);
                throw result.FinalException;
            }

            Logger.Verbose($"{ClassName}::Updated Index to {latestIndex}", this, latestIndex);
        }

        protected override async Task<long> GetLastIndexAsync()
        {
            var index = IndexCacheCollection
                .AsQueryable()
                .FirstOrDefault(i => i.ProjectionName == Name);

            if (index != null)
                return index.LastSeenIndex;

            index = new ProjectionCacheItem
            {
                ProjectionName = Name,
                LastSeenIndex = StartFromBeginning
            };

            await IndexCacheCollection
                .InsertOneAsync(index)
                .ConfigureAwait(false);

            return index.LastSeenIndex;
        }

        protected override async Task UpdateIndexAsync(long latestIndex)
        {
            var index = new ProjectionCacheItem
            {
                ProjectionName = Name,
                LastSeenIndex = latestIndex
            };

            // conditionally update the last seen index if this one is greater than the one currently on file
            var updateDef = Builders<ProjectionCacheItem>.Update.Max(x => x.LastSeenIndex, index.LastSeenIndex);
            var filter = Builders<ProjectionCacheItem>.Filter.Eq(x => x.ProjectionName, index.ProjectionName);
            
            await IndexCacheCollection
                .UpdateOneAsync(filter, updateDef)
                .ConfigureAwait(false);

            Logger.Verbose($"{ClassName}::Mongo index cache was updated. {{lastSeenIndex}}", this, LastIndex);
        }
    }
}
