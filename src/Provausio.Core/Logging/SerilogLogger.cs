using System;
using System.Reflection;
using Provausio.Core.Logging;

namespace Provausio.Core.Logging
{
    public class SerilogLogger : ILogger
    {
        private readonly Serilog.ILogger _logger;
        
        public const string DefaultTextLogEntryFormat = "{MachineName} [{Level:u3} {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [Thread {ThreadId}]  {Message}{Exception}{NewLine}";
        
        /// <summary>
        /// Helper method builds default text log filename/path based on the calling assembly.
        /// </summary>
        /// <param name="callingAssembly"></param>
        /// <returns></returns>
        public static string GetDefaultTextLogFilename(Assembly callingAssembly)
        {
            return $"C:\\Logs\\Provausio\\{callingAssembly.GetName().Name}\\log-{{Date}}.log";
        }

        public SerilogLogger(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void Verbose(object message, object context, params object[] parameters)
        {
            _logger.Verbose(Message(message, context), parameters);
        }

        public void Debug(object message, object context, params object[] parameters)
        {
            _logger.Debug(Message(message, context), parameters);
        }

        public void Information(object message, object context, params object[] parameters)
        {
            _logger.Information(message.ToString(), parameters);
        }

        public void Warning(object message, object context, params object[] parameters)
        {
            _logger.Warning(message.ToString(), parameters);
        }

        public void Error(object message, object context, params object[] parameters)
        {
            _logger.Error(message.ToString(), parameters);
        }

        public void Fatal(object message, object context, Exception exception, params object[] parameters)
        {
            _logger.Fatal(exception, message.ToString(), parameters);
        }

        private static string Message(object message, object context)
        {
            return context == null 
                ? message.ToString() 
                : $"{context.GetType().Name}::{message}";
        }
    }
}