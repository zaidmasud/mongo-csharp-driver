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
    /// Allows for reads and writes to go to different locations.
    /// </summary>
    public sealed class EventuallyConsistentSession : SessionBase
    {
        // private fields
        private readonly ISession _querySession;
        private readonly ISession _nonQuerySession;
        private int _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EventuallyConsistentSession" /> class.
        /// </summary>
        /// <param name="querySession">The query session.</param>
        /// <param name="nonQuerySession">The non query session.</param>
        public EventuallyConsistentSession(ISession querySession, ISession nonQuerySession)
        {
            Ensure.IsNotNull("querySession", querySession);
            Ensure.IsNotNull("nonQuerySession", nonQuerySession);

            _querySession = querySession;
            _nonQuerySession = nonQuerySession;
        }

        // public methods
        /// <summary>
        /// Executes the specified operation.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public override T Execute<T>(IOperation<T> operation, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull("operation", operation);
            ThrowIfDisposed();

            if (operation.IsQuery)
            {
                return _querySession.Execute(operation, timeout, cancellationToken);
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
                    _querySession.Dispose();
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