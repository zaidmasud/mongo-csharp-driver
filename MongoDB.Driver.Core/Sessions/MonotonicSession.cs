using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// A monotonic session.  Uses a given session for queries until a non-query occurs, and then all queries use the non-query session.
    /// </summary>
    public sealed class MonotonicSession : SessionBase
    {
        // private fields
        private readonly ISession _nonQuerySession;
        private ISession _currentSession;
        private int _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MonotonicSession" /> class.
        /// </summary>
        /// <param name="querySession">The query session.</param>
        /// <param name="nonQuerySession">The non query session.</param>
        public MonotonicSession(ISession querySession, ISession nonQuerySession)
        {
            Ensure.IsNotNull("querySession", querySession);
            Ensure.IsNotNull("nonQuerySession", nonQuerySession);

            _currentSession = querySession;
            _nonQuerySession = nonQuerySession;
        }

        // public methods
        /// <summary>
        /// Executes the specified operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The results of the operation.</returns>
        public override T Execute<T>(IOperation<T> operation, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull("operation", operation);
            ThrowIfDisposed();

            var sessionToUse = _currentSession;
            if (operation.IsQuery)
            {
                return sessionToUse.Execute(operation, timeout, cancellationToken);
            }

            var old = Interlocked.CompareExchange(ref _currentSession, _nonQuerySession, sessionToUse);
            if (old == sessionToUse)
            {
                // we can dispose of this one early even though it would get disposed later too...
                old.Dispose();
            }
            return _nonQuerySession.Execute(operation, timeout, cancellationToken);
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                try
                {
                    _currentSession.Dispose();
                }
                catch { } // nothing we can do at this point...
                try
                {
                    _nonQuerySession.Dispose();
                }
                catch { } // nothing we can do at this point...
            }
        }

        // private methods
        private void ThrowIfDisposed()
        {
            if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}