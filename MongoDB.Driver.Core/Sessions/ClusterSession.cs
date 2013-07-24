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
    /// A ClusterSession picks a new server for every operation.
    /// </summary>
    public sealed class ClusterSession : ClusterSessionBase
    {
        // private fields
        private readonly ICluster _cluster;
        private bool _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public ClusterSession(ICluster cluster)
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

            var server = _cluster.SelectServer(args.ServerSelector, args.Timeout, args.CancellationToken);

            return new ServerChannelProvider(this, server, args.DisposeSession);
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
    }
}