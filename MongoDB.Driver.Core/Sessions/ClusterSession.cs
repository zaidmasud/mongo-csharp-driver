using System;
using System.Threading;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// A session based on an entire cluster.
    /// </summary>
    public sealed class ClusterSession : ClusterSessionBase
    {
        // private fields
        private readonly ICluster _cluster;
        private readonly SessionSettings _settings;
        private bool _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public ClusterSession(ICluster cluster)
            : this(cluster, new SessionSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="settings">The settings.</param>
        public ClusterSession(ICluster cluster, SessionSettings settings)
        {
            Ensure.IsNotNull("cluster", cluster);
            Ensure.IsNotNull("settings", settings);

            _cluster = cluster;
            _settings = settings;
        }

        // public methods
        /// <summary>
        /// Creates an operation channel provider.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>An operation channel provider.</returns>
        public override IOperationChannelProvider CreateOperationChannelProvider(CreateOperationChannelProviderOptions options)
        {
            Ensure.IsNotNull("options", options);
            ThrowIfDisposed();

            var server = _cluster.SelectServer(options.ServerSelector, _settings.Timeout, _settings.CancellationToken);
            return new ClusterOperationChannelProvider(this, server, _settings.Timeout, _settings.CancellationToken, options.DisposeSession);
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            _disposed = true;
        }

        // private methods
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        // nested classes
        private sealed class ClusterOperationChannelProvider : IOperationChannelProvider
        {
            private readonly ClusterSession _session;
            private readonly IServer _server;
            private readonly TimeSpan _timeout;
            private readonly CancellationToken _cancellationToken;
            private readonly bool _disposeSession;
            private bool _disposed;

            public ClusterOperationChannelProvider(ClusterSession session, IServer server, TimeSpan timeout, CancellationToken cancellationToken, bool disposeSession)
            {
                _session = session;
                _server = server;
                _timeout = timeout;
                _cancellationToken = cancellationToken;
                _disposeSession = disposeSession;
            }

            public ServerDescription Server
            {
                get 
                {
                    ThrowIfDisposed();
                    return _server.Description; 
                }
            }

            public IServerChannel GetChannel()
            {
                ThrowIfDisposed();
                return _server.GetChannel(_timeout, _cancellationToken);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _server.Dispose();
                    if (_disposeSession)
                    {
                        _session.Dispose();
                    }
                    GC.SuppressFinalize(this);
                }
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                _session.ThrowIfDisposed();
            }
        }
    }
}