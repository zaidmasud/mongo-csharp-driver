using System;
using System.Linq;
using System.Threading;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// A session bound to a particular server.
    /// </summary>
    public sealed class SingleServerSession : ClusterSessionBase
    {
        // private fields
        private readonly SessionBehavior _behavior;
        private readonly ICluster _cluster;
        private bool _disposed;
        private IServer _nonQueryServer;
        private IServer _queryServer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleServerSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public SingleServerSession(ICluster cluster)
            : this(cluster, SessionBehavior.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleServerSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="behavior">The behavior.</param>
        public SingleServerSession(ICluster cluster, SessionBehavior behavior)
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

            IServer serverToUse;
            if (args.IsQuery)
            {
                serverToUse = GetServerForQuery(args);
            }
            else
            {
                serverToUse = GetServerForNonQuery(args);
            }

            // verify that the server selector for the operation is compatible with the selected server.
            var selected = args.ServerSelector.SelectServers(new[] { serverToUse.Description });
            if (!selected.Any())
            {
                throw new Exception("The current operation does not match the selected server.");
            }

            return new SingleServerOperationChannelProvider(this, serverToUse, args.DisposeSession);
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
                if (_nonQueryServer != null)
                {
                    _nonQueryServer.Dispose();
                }
                if (_queryServer != null)
                {
                    _queryServer.Dispose();
                }
                _disposed = true;
            }
        }

        //private methods
        private IServer GetServerForNonQuery(CreateServerChannelProviderArgs args)
        {
            if (_queryServer != null)
            {
                // we want to use the query server if it is a primary.  This is especially 
                // important if we are talking to a mongos which shouldn't switch servers
                // ever.
                var matches = PrimaryServerSelector.Instance.SelectServers(new[] { _queryServer.Description });
                if (matches.Any())
                {
                    _nonQueryServer = _queryServer;
                }
            }

            // if we've never done a non-query or a query with the primary
            if (_nonQueryServer == null)
            {
                _nonQueryServer = _cluster.SelectServer(PrimaryServerSelector.Instance, args.Timeout, args.CancellationToken);
            }

            var serverToUse = _nonQueryServer;

            // if we are monotonic and the query server isn't the non-query server, 
            // we need to change the query server over to the non-query server.
            if (_behavior == SessionBehavior.Monotonic)
            {
                if (_queryServer != null && _queryServer != serverToUse)
                {
                    _queryServer.Dispose();
                }
                _queryServer = serverToUse;
            }

            return serverToUse;
        }

        private IServer GetServerForQuery(CreateServerChannelProviderArgs args)
        {
            if (_queryServer == null)
            {
                _queryServer = _cluster.SelectServer(args.ServerSelector, args.Timeout, args.CancellationToken);
            }
            return _queryServer;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        // nested classes
        private sealed class SingleServerOperationChannelProvider : IServerChannelProvider
        {
            private readonly SingleServerSession _session;
            private readonly IServer _server;
            private readonly bool _disposeSession;
            private bool _disposed;

            public SingleServerOperationChannelProvider(SingleServerSession session, IServer server, bool disposeSession)
            {
                _session = session;
                _server = server;
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

            public IChannel GetChannel(TimeSpan timeout, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _server.GetChannel(timeout, cancellationToken);
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