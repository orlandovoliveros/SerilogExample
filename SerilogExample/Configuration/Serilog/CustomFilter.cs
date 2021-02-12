using Serilog.Core;
using Serilog.Events;

namespace SerilogExample
{
    // The filter syntax in the sample configuration file is processed by the Serilog.Filters.Expressions package.
    public class CustomFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return true;
        }
    }
}
