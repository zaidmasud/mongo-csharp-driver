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
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// A DualServerSession uses one server for writes and a possibly different server for reads.
    /// </summary>
    public sealed class DualServerSession : ClusterSessionBase
    {
        // private fields
        private readonly ICluster _cluster;
        private bool _disposed;
        private IServer _nonQueryServer;
        private IServer _queryServer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DualServerSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public DualServerSession(ICluster cluster)
        {
            Ensure.IsNotNull("cluster", cluster);

            _cluster = cluster;
        }

        // public methods
        /// <summary>
        /// Creates a server channel provider.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>A server channel provider.</returns>
        public override IServerChannelProvider CreateServerChannelProvider(CreateServerChannelProviderArgs args)
        {
            Ensure.IsNotNull("args", args);
            ThrowIfDisposed();

            IServer server;
            if (args.IsQuery)
            {
                server = GetServerForQuery(args);
            }
            else
            {
                server = GetServerForNonQuery(args);
            }

            // verify that the server selector for the operation is compatible with the selected server.
            var selected = args.ServerSelector.SelectServers(new[] { server.Description });
            if (!selected.Any())
            {
                throw new Exception("The current operation does not match the selected server.");
            }

            return new ServerChannelProvider(this, server, args.DisposeSession);
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
            return _nonQueryServer;
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
    }
}