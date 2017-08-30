using Provausio.Core.Ext;
using Serilog.Core;
using Serilog.Events;

namespace Provausio.Core.Logging
{
    /// <summary>
    /// Provides a Runtime control for the Serilog logger implementation. 
    /// </summary>
    public class SerilogControl : ILoggerControl
    {
        public string CurrentLogLevel { get; private set; }

        public LoggingLevelSwitch LogLevelSwitch { get; }

        public SerilogControl()
            : this(LogEventLevel.Information)
        {
        }

        public SerilogControl(LogEventLevel initialLevel)
        {
            LogLevelSwitch = new LoggingLevelSwitch(initialLevel);
            CurrentLogLevel = initialLevel.ToString();
        }

        public void SetLogLevel(string level)
        {
            if (!level.TryFindEnum(out LogEventLevel logLevel))
                return;

            LogLevelSwitch.MinimumLevel = logLevel;
            CurrentLogLevel = level;
        }
    }
}