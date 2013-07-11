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
    public sealed class SingleChannelSession : ClusterSessionBase
    {
        // private fields
        private readonly ICluster _cluster;
        private readonly SessionSettings _settings;
        private IServerChannel _channel;
        private bool _disposed;
        private IServer _server;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChannelSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public SingleChannelSession(ICluster cluster)
            : this(cluster, new SessionSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChannelSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="settings">The settings.</param>
        public SingleChannelSession(ICluster cluster, SessionSettings settings)
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

            if (_server == null)
            {
                _server = _cluster.SelectServer(options.ServerSelector, _settings.Timeout, _settings.CancellationToken);
                _channel = _server.GetChannel(_settings.Timeout, _settings.CancellationToken);
            }

            var selected = options.ServerSelector.SelectServers(new[] { _server.Description });
            if (selected.Any())
            {
                throw new Exception("The current operation does not match the selected channel.");
            }

            return new SingleChannelOperationChannelProvider(this, _channel, options.DisposeSession);
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_server != null)
                {
                    _channel.Dispose();
                    _server.Dispose();
                }
                _disposed = true;
            }
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
        private sealed class SingleChannelOperationChannelProvider : IOperationChannelProvider
        {
            private readonly SingleChannelSession _session;
            private readonly IServerChannel _channel;
            private bool _disposeSession;
            private bool _disposed;

            public SingleChannelOperationChannelProvider(SingleChannelSession session, IServerChannel channel, bool disposeSession)
            {
                _session = session;
                _channel = channel;
                _disposeSession = disposeSession;
            }

            public ServerDescription Server
            {
                get 
                {
                    ThrowIfDisposed();
                    return _channel.Server; 
                }
            }

            public IServerChannel GetChannel()
            {
                ThrowIfDisposed();
                return new DisposalProtectedServerChannel(_channel);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
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