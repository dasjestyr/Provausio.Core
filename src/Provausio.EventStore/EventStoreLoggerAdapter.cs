using System;
using EventStore.ClientAPI;

namespace Provausio.EventStore
{
    public class EventStoreLoggerAdapter : ILogger
    {
        private readonly Core.Logging.ILogger _provausioLogger;

        public EventStoreLoggerAdapter(Core.Logging.ILogger provausioLogger)
        {
            _provausioLogger = provausioLogger;
        }

        public void Error(string format, params object[] args)
        {
            _provausioLogger.Error(format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            _provausioLogger.Error(format + ex, null, args);
        }

        public void Info(string format, params object[] args)
        {
            _provausioLogger.Information(format, null, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            _provausioLogger.Information(format + ex, null, args);
        }

        public void Debug(string format, params object[] args)
        {
            _provausioLogger.Debug(format, null, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            _provausioLogger.Debug(format + ex, null, args);
        }
    }
}
