using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace TechTreeCreator.Logger {
    public static class ILoggerExtensions {
        public static ILogger Here(this ILogger logger,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) {
            return logger
                .ForContext("MemberName", memberName)
                .ForContext("FilePath", sourceFilePath)
                .ForContext("LineNumber", sourceLineNumber);
        }
    }
    
    public class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var skip = 3;
            while (true)
            {
                var stack = new StackFrame(skip);
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown type>")));
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("CallerMethod", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();
                if (method.DeclaringType.Assembly != typeof(Log).Assembly)
                {
                    var caller = $"{method.DeclaringType.FullName}";
                    var callerShort = $"{method.DeclaringType.Name}";
                    var methodName = $"{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.FullName))})";
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("ShortCaller", new ScalarValue(callerShort)));
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("CallerMethod", new ScalarValue(methodName)));
                }

                skip++;
            }
        }
    }

    public static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
    }
}