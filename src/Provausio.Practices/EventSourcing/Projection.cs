using System;
using System.Threading.Tasks;
using Provausio.Core.Logging;
using Provausio.Practices.Validation.Assertion;

namespace Provausio.Practices.EventSourcing
{
    public abstract class Projection<T> : IProjection
        where T : class
    {
        protected const string IndexCacheName = "ProjectionIndexCache";

        private readonly T _client;
        
        protected readonly ISubscriptionAdapter SubscriptionAdapter;
        protected readonly string ClassName;
        
        /// <summary>
        /// If true, this projection will not actually start when Start() is called. Used for testing purposes.
        /// </summary>
        public bool BypassStart { get; set; }

        /// <summary>
        /// Gets the name of the projection. This is used to identify it in the cache.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the last recorded index.
        /// </summary>
        /// <value>
        /// The last index.
        /// </value>
        public long LastIndex { get; private set; }

        protected Projection(
            string name, 
            ISubscriptionAdapter subscriptionAdapter, T client) 
        {
            _client = Ensure.IsNotNull(client, nameof(client));

            Name = Ensure.IsNotNullOrEmpty(name, nameof(name));
            SubscriptionAdapter = Ensure.IsNotNull(subscriptionAdapter, nameof(subscriptionAdapter));
            ClassName = GetType().Name;
        }

        /// <summary>
        /// Performs the provide save action and updates the index.
        /// </summary>
        /// <param name="saveAction">The save action.</param>
        /// <param name="latestIndex">Index of the latest.</param>
        /// <returns></returns>
        public async Task SaveAsync(Func<T, Task> saveAction, long latestIndex)
        {
            try
            {
                await OnSaveAsync(saveAction, latestIndex, _client).ConfigureAwait(false);

                Logger.Verbose($"{ClassName}::{latestIndex} - Done! Updating index cache...", this);

                await UpdateIndexAsync(latestIndex).ConfigureAwait(false);

                Logger.Verbose($"{ClassName}::{latestIndex} - Done!", this);
                LastIndex = latestIndex;
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ClassName}::{latestIndex} - Save action failed.", this, ex);
                throw;
            }
        }

        /// <summary>
        /// Overriding this method allows the child class to control wrap 
        /// the save action passed into SaveAsync (e.g. add retry logic, etc.) 
        /// while still honoring things like the throttle.
        /// </summary>
        /// <param name="saveAction">The same save action that was given to SaveAsync(...)</param>
        /// <param name="latestIndex"></param>
        /// <param name="client">The instance of <typeparamref name="{T}"/> that was supplied to the ctor</param>
        /// <returns></returns>
        protected virtual async Task OnSaveAsync(Func<T, Task> saveAction, long latestIndex, T client)
        {
            Logger.Verbose($"{ClassName}::{latestIndex} - Saving...", this);
            await saveAction(_client).ConfigureAwait(false);
        }

        protected async Task InitializeAsync()
        {
            Logger.Verbose($"Running projection initialization... \"{GetType().Name}\"...", this);
            Logger.Verbose("Fetching last known index...", this);
            LastIndex = await GetLastIndexAsync().ConfigureAwait(false);
            Logger.Verbose($"Found Index {LastIndex}", this);
            Logger.Verbose("Initialization completed...", this);
        }

        protected async void ProcessEvent(EventInfo e, long index)
        {
            if (LastIndex >= index)
            {
                Logger.Verbose("Index is less than last recorded {@index}", this, new { LastIndex, ReceivedIndex = index });
                return;
            }

            try
            {
                var t = this.AsDynamic().Process(e, index) as Task;
                if (t != null) await t;

                Logger.Verbose($"{ClassName}::Dynamically invoked {{@Data}}", this, new
                {
                    EventType = e.GetType().Name,
                    TargetObject = GetType().Name
                });
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ClassName}::Could not dynamically call child process.", this, ex, e);
                throw;
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public abstract Task StartAsync();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public abstract Task StopAsync();

        /// <summary>
        /// Retrieves the last known index.
        /// </summary>
        /// <returns></returns>
        protected abstract Task<long> GetLastIndexAsync();

        /// <summary>
        /// Updates the last known index with the cache.
        /// </summary>
        /// <param name="latestIndex"></param>
        protected abstract Task UpdateIndexAsync(long latestIndex);
    }
}
