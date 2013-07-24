﻿/* Copyright 2010-2013 10gen Inc.
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
    /// A SingleServerSession picks a server when the first operation is executed and uses that single server for all subsequent operations.
    /// </summary>
    public sealed class SingleServerSession : ClusterSessionBase
    {
        // private fields
        private readonly ICluster _cluster;
        private bool _disposed;
        private IServer _server;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleServerSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public SingleServerSession(ICluster cluster)
        {
            Ensure.IsNotNull("cluster", cluster);

            _cluster = cluster;
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

            if (_server == null)
            {
                _server = _cluster.SelectServer(args.ServerSelector, args.Timeout, args.CancellationToken);
            }

            // verify that the server selector for the operation is compatible with the selected server.
            var selected = args.ServerSelector.SelectServers(new[] { _server.Description });
            if (!selected.Any())
            {
                throw new Exception("The current operation does not match the selected server.");
            }

            return new ServerChannelProvider(this, _server, args.DisposeSession);
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
                    _server.Dispose();
                }
                _disposed = true;
            }
        }

        //private methods
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}