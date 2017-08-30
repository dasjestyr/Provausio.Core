using System;
using System.Threading;
using System.Threading.Tasks;
using Provausio.Core.Logging;

namespace Provausio.Core
{

    /// <summary>
    /// Provides a means of executing a task that is limited by a <seealso cref="SemaphoreSlim"/>
    /// </summary>
    public class ActionSemaphore
    {
        private readonly TimeSpan _waitTimeout;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Initializes a new instance of <see cref="ActionSemaphore"/>
        /// </summary>
        /// <param name="maxConcurrency">The max number of tasks that can be scheduled at the same time.</param>
        /// <param name="waitTimeout">The maximum amount of time that the action should wait for a slot.</param>
        public ActionSemaphore(int maxConcurrency, TimeSpan waitTimeout)
        {
            _waitTimeout = waitTimeout;
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        /// <summary>
        /// Executes the action once a slot is available within the instance.
        /// </summary>
        /// <param name="action">The task that will be executed.</param>
        /// <param name="context">The object that is asking for the execution. Used for logging.</param>
        /// <returns></returns>
        public Task ExecuteAsync(Func<Task> action, object context)
        {
            // no need to release on this exception because a slot was never claimed
            if (!_semaphore.Wait(_waitTimeout))
                throw new TimeoutException($"{context.GetType().Name} timed out while waiting for a slot.");

            try
            {
                return action().ContinueWith(t => _semaphore.Release());
            }
            catch (Exception ex)
            {
                // make sure to open the slot if the actions throws
                Logger.Fatal($"{ex.Message}", this, ex);
                _semaphore.Release();
                throw;
            }
        }
    }

    /// <summary>
    /// Marker interface for action instance instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IActionSemaphore<T>
    {
        /// <summary>
        /// Executes the action once a slot is available within the instance.
        /// </summary>
        /// <param name="action">The task that will be executed.</param>
        /// <param name="context">The object that is asking for the execution. Used for logging.</param>
        /// <returns></returns>
        Task ExecuteAsync(Func<Task> action, object context);
    }

    /// <summary>
    /// Adapter provides the means to adapt an <see cref="ActionSemaphore"/> instance to the <see cref="IActionSemaphore{T}"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionSemaphoreAdapter<T> : IActionSemaphore<T>
    {
        private readonly ActionSemaphore _instance;

        /// <summary>
        /// Initializes a new instance of <see cref="ActionSemaphoreAdapter{T}"/>
        /// </summary>
        /// <param name="instance">The action instance.</param>
        public ActionSemaphoreAdapter(ActionSemaphore instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Executes the action once a slot is available within the instance.
        /// </summary>
        /// <param name="action">The task that will be executed.</param>
        /// <param name="context">The object that is asking for the execution. Used for logging.</param>
        /// <returns></returns>
        public Task ExecuteAsync(Func<Task> action, object context)
        {
            return _instance.ExecuteAsync(action, context);
        }
    }
}
