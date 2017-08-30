using Serilog.Core;

namespace Provausio.Core.Logging
{
    public interface ILoggerControl
    {
        string CurrentLogLevel { get; }

        void SetLogLevel(string level);
    }
}