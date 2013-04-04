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
using MongoDB.Driver.Internal;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a wrapped connection.
    /// </summary>
    public class ConnectionWrapper : IDisposable
    {
        // private fields
        private readonly MongoServer _cluster;
        private readonly MongoNode _node;
        private readonly MongoConnection _connection;
        private readonly ConnectionWrapper _chainedWrapper;
        private bool _disposed;

        // constructors
        internal ConnectionWrapper(MongoServer cluster, MongoNode node, MongoConnection connection)
        {
            if (cluster == null)
            {
                throw new ArgumentNullException("cluster");
            }
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            _cluster = cluster;
            _node = node;
            _connection = connection;
        }

        private ConnectionWrapper(ConnectionWrapper chainedWrapper)
        {
            if (chainedWrapper == null)
            {
                throw new ArgumentNullException("chainedWrapper");
            }

            _cluster = chainedWrapper.Cluster;
            _node = chainedWrapper.Node;
            _chainedWrapper = chainedWrapper;
        }

        // public properties
        /// <summary>
        /// Gets the cluster.
        /// </summary>
        /// <value>
        /// The cluster.
        /// </value>
        public MongoServer Cluster
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("ConnectionWrapper"); }
                return _cluster;
            }
        }

        /// <summary>
        /// Gets the inner connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">ConnectionWrapped</exception>
        public MongoConnection Inner
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("ConnectionWrapper"); }
                return _connection ?? _chainedWrapper.Inner;
            }
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// The node.
        /// </value>
        public MongoNode Node
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("ConnectionWrapper"); }
                return _node;
            }
        }

        // public methods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Rewraps the current connection (so the original wrapper owns the connection, not the rewrapped one).
        /// </summary>
        /// <returns>
        /// A connection wrapper.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">selector</exception>
        /// <exception cref="System.ObjectDisposedException">ConnectionWrapper</exception>
        public ConnectionWrapper Rewrap()
        {
            if (_disposed) { throw new ObjectDisposedException("ConnectionWrapper"); }

            return new ConnectionWrapper(this); // wrap this binding
        }

        // private methods
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    _connection.Release();
                }
                _disposed = true;
            }
        }
    }
}
