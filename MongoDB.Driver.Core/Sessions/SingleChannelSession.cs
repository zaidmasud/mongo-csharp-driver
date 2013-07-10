using System;
using System.Linq;
using System.Threading;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// Session that uses a single channel.
    /// </summary>
    public sealed class SingleChannelSession : SessionBase
    {
        // private fields
        private readonly IServerChannel _channel;
        private int _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChannelSession" /> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public SingleChannelSession(IServerChannel channel)
        {
            Ensure.IsNotNull("channel", channel);

            _channel = channel;
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

            var servers = operation.ServerSelector.SelectServers(new[] { _channel.Server });
            if (!servers.Any())
            {
                throw new InvalidOperationException(string.Format("The current channel does not match the required server selector of {0}", operation.ServerSelector));
            }

            var provider = new SingleChannelOperationChannelProvider(_channel);
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
                _channel.Dispose();
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
        private sealed class SingleChannelOperationChannelProvider : IOperationChannelProvider
        {
            private readonly IServerChannel _channel;
            private bool _disposed;

            public SingleChannelOperationChannelProvider(IServerChannel channel)
            {
                _channel = channel;
            }

            public ServerDescription Server
            {
                get 
                {
                    if (_disposed)
                    {
                        throw new ObjectDisposedException(GetType().Name);
                    }
                    return _channel.Server; 
                }
            }

            public IServerChannel GetChannel()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                return new DisposalProtectedServerChannel(_channel);
            }

            public void Dispose()
            {
                _disposed = true;
            }
        }
    }
}