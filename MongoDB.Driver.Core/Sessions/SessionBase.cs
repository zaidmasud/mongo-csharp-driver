using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// Abstract base class for sessions.
    /// </summary>
    public abstract class SessionBase : ISession
    {
        // constructors
        /// <summary>
        /// Finalizes an instance of the <see cref="SessionBase" /> class.
        /// </summary>
        ~SessionBase()
        {
            Dispose(false);
        }

        // public methods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Executes the specified operation.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public abstract T Execute<T>(IOperation<T> operation, TimeSpan timeout, CancellationToken cancellationToken);

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // nothing to do...
        }
    }
}
