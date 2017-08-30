using System;

namespace Provausio.Core.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Verbose log. Use this to emit fine-grained debugging information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        void Verbose(object message, object context, params object[] parameters);

        /// <summary>
        /// Debug logging. Use this to emit logs that contain coarse-grained debugging information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        void Debug(object message, object context, params object[] parameters);

        /// <summary>
        /// Information. Use to log informational messages that are not errors.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        void Information(object message, object context, params object[] parameters);

        /// <summary>
        /// Warning. Use when something non-ideal needs to be logged but has not caused an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        void Warning(object message, object context, params object[] parameters);

        /// <summary>
        /// Error message. Use when an error condition is found or an exception was handled.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        void Error(object message, object context, params object[] parameters);

        /// <summary>
        /// Fatal exception. Use when an unexpected exception was encountered.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="parameters">The parameters.</param>
        void Fatal(object message, object context, Exception exception, params object[] parameters);
    }
}
