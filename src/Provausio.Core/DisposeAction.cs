using System;

namespace Provausio.Core
{
    /// <summary>
    /// Disposable object wrapper.
    /// </summary>
    public class DisposeAction : IDisposable
    {
        private readonly Action _disposeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="IDisposable"/> class.
        /// </summary>
        /// <param name="disposeAction">The dispose action.</param>
        public DisposeAction(Action disposeAction)
        {
            if (disposeAction == null)
                throw new ArgumentNullException(nameof(disposeAction));

            _disposeAction = disposeAction;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposeAction();
        }
    }
}
