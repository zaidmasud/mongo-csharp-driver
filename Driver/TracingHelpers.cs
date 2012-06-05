/* Copyright 2010-2012 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MongoDB.Driver
{
    internal static class TraceSources
    {
        public const string GeneralTraceSourceName = "MongoDB.Driver";
        public const string DataTraceSourceName = "MongoDB.Driver.Data";

        public static TraceSource CreateGeneralTraceSource()
        {   
            return new TraceSource(GeneralTraceSourceName, SourceLevels.Warning);
        }

        public static TraceSource CreateDataTraceSource()
        {
            return new TraceSource(DataTraceSourceName, SourceLevels.Warning);
        }
    }

    internal static class TraceSourceExtensionMethods
    {
        public static void TraceException(this TraceSource traceSource, TraceEventType eventType, Exception ex)
        {
            traceSource.TraceException(eventType, ex, "");
        }

        public static void TraceException(this TraceSource traceSource, TraceEventType eventType, Exception ex, string comment)
        {
            if (traceSource.Switch.ShouldTrace(eventType))
            {
                if (string.IsNullOrEmpty(comment))
                {
                    comment = "";
                }
                else if (!comment.EndsWith(Environment.NewLine))
                {
                    comment += Environment.NewLine;
                }

                traceSource.TraceEvent(eventType, 0, "{0}ExceptionType: {1}{2}Message: {3}",
                    comment, ex.GetType(), Environment.NewLine, ex.Message);

                if ((traceSource.Switch.Level & SourceLevels.Verbose) != 0)
                {
                    // this will print the stack trace
                    traceSource.TraceEvent(TraceEventType.Verbose, 0, ex.ToString());
                }
            }
        }

        public static void TraceException(this TraceSource traceSource, TraceEventType eventType, Exception ex, string format, params object[] args)
        {
            if (traceSource.Switch.ShouldTrace(eventType))
            {
                var comment = string.Format(format, args);
                TraceException(traceSource, eventType, ex, comment);
            }
        }

        public static IDisposable TraceStart(this TraceSource traceSource, string format, params object[] args)
        {
            Guid oldActivityId = Trace.CorrelationManager.ActivityId;
            if (oldActivityId == Guid.Empty)
            {
                Guid newActivityId = Guid.NewGuid();
                Trace.CorrelationManager.ActivityId = newActivityId;
                traceSource.TraceEvent(TraceEventType.Start, 0, format, args);

                return new DisposableAction(
                    () =>
                    {
                        traceSource.TraceEvent(TraceEventType.Stop, 0, format, args);
                        Trace.CorrelationManager.ActivityId = oldActivityId;
                    });
            }
            else
            {
                return null;
            }
        }

        public static IDisposable TraceStartNewActivity(this TraceSource traceSource, string format, params object[] args)
        {
            Guid oldActivityId = Trace.CorrelationManager.ActivityId;
            Guid newActivityId = Guid.NewGuid();
            Trace.CorrelationManager.ActivityId = newActivityId;
            traceSource.TraceEvent(TraceEventType.Start, 0, format, args);

            return new DisposableAction(
                () =>
                {
                    traceSource.TraceEvent(TraceEventType.Stop, 0, format, args);
                    Trace.CorrelationManager.ActivityId = oldActivityId;
                });
        }

        public static void TraceVerbose(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }

        public static void TraceVerbose(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Verbose, 0, message);
        }

        public static void TraceWarning(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        public static void TraceWarning(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Warning, 0, message);
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action _action;
            private bool _disposed;

            public DisposableAction(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    try
                    {
                        _action();
                    }
                    catch
                    {
                        // ignore exceptions (just in case, action isn't supposed to throw exceptions)
                    }
                    _disposed = true;
                }
            }
        }
    }
}
