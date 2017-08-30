using System;
using System.Diagnostics;
using EventStore.ClientAPI;

namespace Provausio.EventStore
{
    public class TraceLogger : ILogger
    {
        public void Error(string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Trace.WriteLine(string.Format(format, args));
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            Trace.WriteLine(string.Format(format, args));
        }
    }
}