using System;
using System.Threading;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// Abstract base class for sessions.
    /// </summary>
    public abstract class ClusterSessionBase : ISession
    {
        // constructors
        /// <summary>
        /// Finalizes an instance of the <see cref="ClusterSessionBase" /> class.
        /// </summary>
        ~ClusterSessionBase()
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
        /// Creates an operation channel provider.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>An operation channel provider.</returns>
        public abstract IOperationChannelProvider CreateOperationChannelProvider(CreateOperationChannelProviderOptions options);

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