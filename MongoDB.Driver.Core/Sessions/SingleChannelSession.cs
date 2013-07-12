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
        private readonly SessionBehavior _behavior;
        private readonly ICluster _cluster;
        private bool _disposed;
        private IServerChannel _nonQueryChannel;
        private IServer _nonQueryServer;
        private IServerChannel _queryChannel;
        private IServer _queryServer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChannelSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public SingleChannelSession(ICluster cluster)
            : this(cluster, SessionBehavior.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChannelSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="behavior">The behavior.</param>
        public SingleChannelSession(ICluster cluster, SessionBehavior behavior)
        {
            Ensure.IsNotNull("cluster", cluster);

            _cluster = cluster;
            _behavior = behavior;
        }

        // public methods
        /// <summary>
        /// Creates an operation channel provider.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>An operation channel provider.</returns>
        public override ISessionChannelProvider CreateSessionChannelProvider(CreateSessionChannelProviderOptions options)
        {
            Ensure.IsNotNull("options", options);
            ThrowIfDisposed();

            IServerChannel channelToUse;
            if (!options.IsQuery)
            {
                if (_queryServer != null)
                {
                    // we want to use the query channel if it matches
                    var matches = PrimaryServerSelector.Instance.SelectServers(new[] { _queryServer.Description }).Any();
                    if (matches)
                    {
                        _nonQueryServer = _queryServer;
                        _nonQueryChannel = _queryChannel;
                    }
                }

                if (_nonQueryServer == null)
                {
                    _nonQueryServer = _cluster.SelectServer(PrimaryServerSelector.Instance, options.Timeout, options.CancellationToken);
                    _nonQueryChannel = _nonQueryServer.GetChannel(options.Timeout, options.CancellationToken);
                }
                channelToUse = _nonQueryChannel;

                if (_behavior == SessionBehavior.Monotonic)
                {
                    if (_queryChannel != null && _queryChannel != channelToUse)
                    {
                        _queryChannel.Dispose();
                        _queryServer.Dispose();
                    }
                    _queryServer = _nonQueryServer;
                    _queryChannel = _nonQueryChannel;
                }
            }
            else
            {
                if (_queryServer == null)
                {
                    _queryServer = _cluster.SelectServer(options.ServerSelector, options.Timeout, options.CancellationToken);
                    _queryChannel = _queryServer.GetChannel(options.Timeout, options.CancellationToken);
                }
                channelToUse = _queryChannel;
            }

            var selected = options.ServerSelector.SelectServers(new[] { channelToUse.Server });
            if (!selected.Any())
            {
                throw new Exception("The current operation does not match the selected channel.");
            }

            return new SingleChannelOperationChannelProvider(this, channelToUse, options.DisposeSession);
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
                if (_nonQueryChannel != null)
                {
                    _nonQueryChannel.Dispose();
                    _nonQueryServer.Dispose();
                }
                if (_queryChannel != null)
                {
                    _queryChannel.Dispose();
                    _queryServer.Dispose();
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
        private sealed class SingleChannelOperationChannelProvider : ISessionChannelProvider
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