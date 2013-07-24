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
    /// A DualChannelSession uses one channel for writes and a possibly different channel for reads.
    /// </summary>
    public sealed class DualChannelSession : ClusterSessionBase
    {
        // private fields
        private readonly ICluster _cluster;
        private bool _disposed;
        private IChannel _nonQueryChannel;
        private IServer _nonQueryServer;
        private IChannel _queryChannel;
        private IServer _queryServer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DualChannelSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public DualChannelSession(ICluster cluster)
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

            Tuple<IServer, IChannel> serverAndChannel;
            if (args.IsQuery)
            {
                serverAndChannel = GetServerAndChannelForQuery(args);
            }
            else
            {
                serverAndChannel = GetServerAndChannelForNonQuery(args);
            }
            var server = serverAndChannel.Item1;
            var channel = serverAndChannel.Item2;

            var selected = args.ServerSelector.SelectServers(new[] { server.Description });
            if (!selected.Any())
            {
                throw new Exception("The current operation does not match the selected channel.");
            }

            return new ChannelChannelProvider(this, server, channel, args.DisposeSession);
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
        private Tuple<IServer, IChannel> GetServerAndChannelForNonQuery(CreateServerChannelProviderArgs args)
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
                if (_nonQueryServer == _queryServer)
                {
                    _nonQueryChannel = _queryChannel;
                }
                else
                {
                    _nonQueryChannel = _nonQueryServer.GetChannel(args.Timeout, args.CancellationToken);
                }
            }
            return Tuple.Create(_nonQueryServer, _nonQueryChannel);
        }

        private Tuple<IServer, IChannel> GetServerAndChannelForQuery(CreateServerChannelProviderArgs args)
        {
            if (_queryChannel == null)
            {
                _queryServer = _cluster.SelectServer(args.ServerSelector, args.Timeout, args.CancellationToken);
                if (_queryServer == _nonQueryServer)
                {
                    _queryChannel = _nonQueryChannel;
                }
                else
                {
                    _queryChannel = _queryServer.GetChannel(args.Timeout, args.CancellationToken);

                }
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
    }
}