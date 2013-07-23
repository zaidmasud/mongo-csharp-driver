/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
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
        private readonly SessionBehavior _behavior;
        private readonly ICluster _cluster;
        private bool _disposed;
        private bool _pinnedToPrimary;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public ClusterSession(ICluster cluster)
            : this(cluster, SessionBehavior.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="behavior">The behavior.</param>
        public ClusterSession(ICluster cluster, SessionBehavior behavior)
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

            _pinnedToPrimary |= _behavior == SessionBehavior.Monotonic && !args.IsQuery;
            var selector = args.ServerSelector;
            if (_pinnedToPrimary)
            {
                selector = PrimaryServerSelector.Instance;
            }

            var serverToUse = _cluster.SelectServer(selector, args.Timeout, args.CancellationToken);

            // verify that the server selector for the operation is compatible with the selected server.
            var selected = args.ServerSelector.SelectServers(new[] { serverToUse.Description });
            if (!selected.Any())
            {
                throw new Exception("The current operation does not match the selected server.");
            }

            return new ClusterOperationChannelProvider(this, serverToUse, args.DisposeSession);
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
        private sealed class ClusterOperationChannelProvider : IServerChannelProvider
        {
            private readonly ClusterSession _session;
            private readonly IServer _server;
            private readonly bool _disposeSession;
            private bool _disposed;

            public ClusterOperationChannelProvider(ClusterSession session, IServer server, bool disposeSession)
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