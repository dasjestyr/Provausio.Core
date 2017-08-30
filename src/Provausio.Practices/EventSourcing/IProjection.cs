using System;
using System.Threading.Tasks;

namespace Provausio.Practices.EventSourcing
{
    public interface IProjection
    {
        /// <summary>
        /// If true, this projection will not actually start when Start() is called. Used for testing purposes.
        /// </summary>
        bool BypassStart { get; }

        /// <summary>
        /// Gets the name of the projection.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        Task StopAsync();
    }
}
