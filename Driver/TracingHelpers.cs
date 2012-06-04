using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MongoDB.Driver
{
    internal static class TracingConstants
    {
        public const string GENERAL = "MongoDB.Driver";
        public const string DATA = "MongoDB.Driver.Data";

        public static TraceSource CreateGeneralTraceSource()
        {   
            return new TraceSource(GENERAL, SourceLevels.Warning);
        }

        public static TraceSource CreateDataTraceSource()
        {
            return new TraceSource(DATA, SourceLevels.Warning);
        }
    }

    internal static class TraceSourceExtensions
    {
        public static void TraceException(this TraceSource traceSource, TraceEventType eventType, Exception ex)
        {
            traceSource.TraceException(eventType, ex, string.Empty);
        }

        public static void TraceException(this TraceSource traceSource, TraceEventType eventType, Exception ex, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
                comment += Environment.NewLine;

            traceSource.TraceEvent(eventType, (int)eventType, 
                    comment + "ExceptionType: {0} {1} Message: {2}", ex.GetType(), Environment.NewLine, ex.Message);

            if (traceSource.Switch.Level == SourceLevels.Verbose || traceSource.Switch.Level == SourceLevels.All)
            {
                //this will print the stack trace...
                traceSource.TraceEvent(TraceEventType.Verbose, (int)TraceEventType.Verbose, ex.ToString());
            }
        }

        public static void TraceException(this TraceSource traceSource, TraceEventType eventType, Exception ex, string format, params object[] args)
        {
            TraceException(traceSource, eventType, ex, string.Format(format, args));
        }

        public static IDisposable TraceStart(this TraceSource traceSource, string format, params object[] args)
        {
            Guid oldActivityId = Trace.CorrelationManager.ActivityId;
            Guid activityId = Guid.NewGuid();
            return new DisposableAction(
                () =>
                {
                    if (oldActivityId != Guid.Empty)
                    {
                        traceSource.TraceTransfer((int)TraceEventType.Transfer, "transfer in", activityId);
                    }
                    Trace.CorrelationManager.ActivityId = activityId;
                    traceSource.TraceEvent(TraceEventType.Start, (int)TraceEventType.Start, format, args);
                },
                () =>
                {
                    if (oldActivityId != Guid.Empty)
                    {
                        traceSource.TraceTransfer((int)TraceEventType.Transfer, "transfer out", oldActivityId);
                    }
                    traceSource.TraceEvent(TraceEventType.Stop, (int)TraceEventType.Stop, format, args);
                    Trace.CorrelationManager.ActivityId = oldActivityId;
                });
        }

        public static IDisposable TraceStartWithoutTransfer(this TraceSource traceSource, string format, params object[] args)
        {
            Guid activityId = Guid.NewGuid();
            Guid oldActivityId = Trace.CorrelationManager.ActivityId;
            return new DisposableAction(
                () =>
                {
                    Trace.CorrelationManager.ActivityId = activityId;
                    traceSource.TraceEvent(TraceEventType.Start, (int)TraceEventType.Start, format, args);
                },
                () =>
                {
                    traceSource.TraceEvent(TraceEventType.Stop, (int)TraceEventType.Stop, format, args);
                    Trace.CorrelationManager.ActivityId = oldActivityId;
                });
        }

        public static void TraceVerbose(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Verbose, (int)TraceEventType.Verbose, message);
        }

        public static void TraceVerbose(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Verbose, (int)TraceEventType.Verbose, format, args);
        }

        public static void TraceWarning(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Warning, (int)TraceEventType.Verbose, message);
        }

        public static void TraceWarning(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Warning, (int)TraceEventType.Verbose, format, args);
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action _stop;

            public DisposableAction(Action start, Action stop)
            {
                _stop = stop;
                start();
            }

            public void Dispose()
            {
                _stop();
            }
        }
    }

    internal class TracingContext : IDisposable
    {
        public TracingContext(object context)
        {
            Trace.CorrelationManager.StartLogicalOperation(context);
        }

        public void Dispose()
        {
            Trace.CorrelationManager.StopLogicalOperation();
        }
    }

}