using System;
using System.Diagnostics;
using Provausio.Core.Logging;

namespace Provausio.Core.Logging
{
    /// <summary>
    /// Logging service locator.
    /// </summary>
    public static class Logger
    {
        public static ILogger Current { get; private set; }

        /// <summary>
        /// Use this to log fine-grained debug information
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Verbose(object message, object context, params object[] parameters)
        {
            if (Current == null)
            {
                Debug(message, context, parameters);
                return;
            }

            Current.Verbose(message, context, parameters);
        }

        /// <summary>
        /// Use this to log coarse-grained debug information
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Debug(object message, object context, params object[] parameters)
        {
            if (Current == null)
            {
                Trace.WriteLine(DefaultFormat(message, "debug"));
                return;
            }

            Current.Debug(message, context, parameters);
        }

        /// <summary>
        /// Logs information messages. Use this to log events such as "User Created with ID 1234"
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Information(object message, object context, params object[] parameters)
        {
            if (Current == null)
            {
                Trace.TraceInformation(DefaultFormat(message, "info").ToString());
                return;
            }

            Current.Information(message, context, parameters);
        }

        /// <summary>
        /// Logs warning messages. Use this to log warnings that are not errors such as "certificate will expire in 4 days".
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Warning(object message, object context, params object[] parameters)
        {
            if (Current == null)
            {
                Trace.TraceWarning(DefaultFormat(message, "warn").ToString());
                return;
            }

            Current.Warning(message, context, parameters);
        }

        /// <summary>
        /// Logs error messages. Use this to log messages that result from some form of HANDLED error where no exception 
        /// was generated and where the application can recover and continue. For example "Cannot execute command because 
        /// the user is mark inactive."
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Error(object message, object context, params object[] parameters)
        {
            if (Current == null)
            {
                Trace.TraceError(DefaultFormat(message, "error").ToString());
                return;
            }

            Current.Error(message, context, parameters);
        }

        /// <summary>
        /// Logs fatal error messages. Use this to log unrecoverable conditions where the process must be aborted, such as exceptions.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Fatal(object message, object context, Exception exception, params object[] parameters)
        {
            if (Current == null)
            {
                var defaultFormat = DefaultFormat($"{message}\r\n{exception.StackTrace}", "fatal").ToString();
                Trace.TraceError(defaultFormat, exception);
                return;
            }

            Current.Fatal(message, context, exception, parameters);
        }

        /// <summary>
        /// Sets the logger implementation.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public static void SetLogger(ILogger logger)
        {
            Current = logger;
        }

        private static object DefaultFormat(object message, string category)
        {
            return $"{DateTimeOffset.Now}\t{category.ToUpper()}: {message}";
        }
    }
}
