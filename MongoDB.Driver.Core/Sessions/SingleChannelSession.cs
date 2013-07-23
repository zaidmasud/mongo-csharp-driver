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
        private IChannel _nonQueryChannel;
        private IServer _nonQueryServer;
        private IChannel _queryChannel;
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
        /// <param name="args">The args.</param>
        /// <returns>An operation channel provider.</returns>
        public override IServerChannelProvider CreateServerChannelProvider(CreateServerChannelProviderArgs args)
        {
            Ensure.IsNotNull("args", args);
            ThrowIfDisposed();

            Tuple<IServer, IChannel> serverChannelToUse;
            if (args.IsQuery)
            {
                serverChannelToUse = GetServerChannelForQuery(args);
            }
            else
            {
                serverChannelToUse = GetServerChannelForNonQuery(args);
            }

            var selected = args.ServerSelector.SelectServers(new[] { serverChannelToUse.Item1.Description });
            if (!selected.Any())
            {
                throw new Exception("The current operation does not match the selected channel.");
            }

            return new SingleChannelOperationChannelProvider(this, serverChannelToUse.Item1, serverChannelToUse.Item2, args.DisposeSession);
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
        private Tuple<IServer, IChannel> GetServerChannelForNonQuery(CreateServerChannelProviderArgs args)
        {
            if (_queryServer != null)
            {
                // we want to use the query channel if it is a primary.  This is especially 
                // important if we are talking to a mongos which shouldn't switch channels
                // ever.
                var matches = PrimaryServerSelector.Instance.SelectServers(new[] { _queryServer.Description }).Any();
                if (matches)
                {
                    _nonQueryServer = _queryServer;
                    _nonQueryChannel = _queryChannel;
                }
            }

            // if we've never done a non-query or a query with the primary
            if (_nonQueryServer == null)
            {
                _nonQueryServer = _cluster.SelectServer(PrimaryServerSelector.Instance, args.Timeout, args.CancellationToken);
                _nonQueryChannel = _nonQueryServer.GetChannel(args.Timeout, args.CancellationToken);
            }
            
            var serverToUse = _nonQueryServer;
            var channelToUse = _nonQueryChannel;

            // if we are monotonic and the query channel isn't the non-query channel, 
            // we need to change the query channel over to the non-query channel.
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

            return Tuple.Create(serverToUse, channelToUse);
        }

        private Tuple<IServer, IChannel> GetServerChannelForQuery(CreateServerChannelProviderArgs args)
        {
            if (_queryServer == null)
            {
                _queryServer = _cluster.SelectServer(args.ServerSelector, args.Timeout, args.CancellationToken);
                _queryChannel = _queryServer.GetChannel(args.Timeout, args.CancellationToken);
            }

            return Tuple.Create(_queryServer, _queryChannel);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        // nested classes
        private sealed class SingleChannelOperationChannelProvider : IServerChannelProvider
        {
            private readonly SingleChannelSession _session;
            private readonly IChannel _channel;
            private readonly IServer _server;
            private bool _disposeSession;
            private bool _disposed;

            public SingleChannelOperationChannelProvider(SingleChannelSession session, IServer server, IChannel channel, bool disposeSession)
            {
                _session = session;
                _server = server;
                _channel = channel;
                _disposeSession = disposeSession;
            }

            public ServerDescription Server
            {
                get { return _server.Description; }
            }

            public IChannel GetChannel(TimeSpan timeout, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return new DisposalProtectedChannel(_channel);
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