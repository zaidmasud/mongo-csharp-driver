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
    /// A session based on an entire cluster.
    /// </summary>
    public sealed class ClusterSession : SessionBase
    {
        // private fields
        private readonly ICluster _cluster;
        private int _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public ClusterSession(ICluster cluster)
        {
            Ensure.IsNotNull("cluster", cluster);

            _cluster = cluster;
        }

        // public methods
        /// <summary>
        /// Executes the specified operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public override T Execute<T>(IOperation<T> operation, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull("operation", operation);
            ThrowIfDisposed();

            using (var server = _cluster.SelectServer(operation.ServerSelector, timeout, cancellationToken))
            {
                var provider = new ClusterOperationChannelProvider(server, timeout, cancellationToken);
                return operation.Execute(provider);
            }
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            Interlocked.CompareExchange(ref _disposed, 1, 0);
        }

        // private methods
        private void ThrowIfDisposed()
        {
            if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        // nested classes
        private class ClusterOperationChannelProvider : IOperationChannelProvider
        {
            private readonly IServer _server;
            private readonly TimeSpan _timeout;
            private readonly CancellationToken _cancellationToken;

            public ClusterOperationChannelProvider(IServer server, TimeSpan timeout, CancellationToken cancellationToken)
            {
                Ensure.IsNotNull("server", server);

                _server = server;
                _timeout = timeout;
                _cancellationToken = cancellationToken;
            }

            public ServerDescription Server
            {
                get { return _server.Description; }
            }

            public IServerChannel GetChannel()
            {
                return _server.GetChannel(_timeout, _cancellationToken);
            }
        }
    }
}