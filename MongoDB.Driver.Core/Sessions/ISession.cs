using System;
using System.Threading;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// A session decides on where operations get executed.
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Executes the specified operation.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation</returns>
        T Execute<T>(IOperation<T> operation, TimeSpan timeout, CancellationToken cancellationToken);
    }
}