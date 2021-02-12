using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;

namespace SerilogExample
{
    public class CustomPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            result = null;

            if (value is LoginData data)
            {
                result = new StructureValue(
                    new List<LogEventProperty>
                    {
                        new LogEventProperty("Username", new ScalarValue(data.Username))
                    });
            }

            return (result != null);
        }
    }
}
