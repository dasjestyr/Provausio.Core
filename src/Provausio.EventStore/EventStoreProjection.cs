using System;
using System.Threading.Tasks;
using Provausio.Core.Logging;
using Provausio.Practices.EventSourcing;
using Provausio.Practices.Validation.Assertion;

namespace Provausio.EventStore
{
    public abstract class EventStoreProjection<T> : Projection<T>
        where T : class
    {
        protected const int StartFromBeginning = -2;
        private readonly string _streamName;
        
        protected Action<ISubscriptionAdapter> OnStartAction;
        
        protected EventStoreProjection(
            string name,
            string streamName,
            ISubscriptionAdapter subscriptionAdapter,
            T client)
            : base(name, subscriptionAdapter, client)
        {
            _streamName = Ensure.IsNotNullOrEmpty(streamName, nameof(streamName));
        }

        /// <summary>
        /// Used to override subscription adapter invocation on projection start.
        /// </summary>
        /// <param name="onStart"></param>
        public void OnStart(Action<ISubscriptionAdapter> onStart)
        {
            OnStartAction = Ensure.IsNotNull(onStart, nameof(onStart));
        }

        public override async Task StartAsync()
        {
            Logger.Verbose($"{ClassName}::Starting projection...", this);
            if (BypassStart)
            {
                Logger.Warning($"{ClassName}::Bypass was set to 'true' projection will not start!", this);
                return;
            }
            
            await InitializeAsync().ConfigureAwait(false);
            if (OnStartAction == null)
            {
                Logger.Verbose($"{ClassName}::Starting default (CatchUp) subscription...", this);
                SubscriptionAdapter.CatchUpSubscription(
                    _streamName,
                    LastIndex,
                    ProcessEvent);
            }
            else
            {
                Logger.Verbose($"{ClassName}::Starting using custom OnStart action...", this);
                OnStartAction(SubscriptionAdapter);
            }
        }

        public override Task StopAsync()
        {
            if (BypassStart)
                return Task.CompletedTask;

            SubscriptionAdapter.Stop();
            return Task.CompletedTask;
        }
    }
}
