using System;
using Orleans.Runtime;
using Serilog.Core;
using Serilog.Events;

namespace OrleansWithWebApp.Extensions
{
    public class TraceIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) { throw new ArgumentNullException(nameof(logEvent)); }
            if (propertyFactory == null) { throw new ArgumentNullException(nameof(propertyFactory)); }
            var traceId = RequestContext.Get("TraceId");
            var property = propertyFactory.CreateProperty("TraceId", traceId);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}