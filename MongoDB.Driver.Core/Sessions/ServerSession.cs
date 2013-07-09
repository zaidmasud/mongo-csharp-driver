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
    /// A session bound to a particular server.
    /// </summary>
    public sealed class ServerSession : SessionBase
    {
        // private fields
        private readonly IServer _server;
        private int _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSession" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        public ServerSession(IServer server)
        {
            Ensure.IsNotNull("server", server);

            _server = server;
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

            var servers = operation.ServerSelector.SelectServers(new[] { _server.Description });
            if (!servers.Any())
            {
                throw new InvalidOperationException(string.Format("The current server does not match the required server selector of {0}", operation.ServerSelector));
            }

            var provider = new ServerOperationChannelProvider(_server, timeout, cancellationToken);
            return operation.Execute(provider);
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
                _server.Dispose();
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

        // nested classes
        private class ServerOperationChannelProvider : IOperationChannelProvider
        {
            private readonly IServer _server;
            private readonly TimeSpan _timeout;
            private readonly CancellationToken _cancellationToken;

            public ServerOperationChannelProvider(IServer server, TimeSpan timeout, CancellationToken cancellationToken)
            {
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