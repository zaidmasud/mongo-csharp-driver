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
    /// Represents a binding to a connection.
    /// </summary>
    public class ConnectionBinding : IMongoBinding, IDisposable
    {
        // private fields
        private readonly MongoServer _cluster;
        private readonly MongoServerInstance _node;
        private readonly MongoConnection _connection;
        private readonly ConnectionBinding _connectionBinding;
        private bool _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionBinding"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="node">The node.</param>
        /// <param name="connection">The connection.</param>
        /// <exception cref="System.ArgumentNullException">
        /// cluster
        /// or
        /// node
        /// or
        /// connection
        /// </exception>
        public ConnectionBinding(MongoServer cluster, MongoServerInstance node, MongoConnection connection)
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionBinding"/> class.
        /// </summary>
        /// <param name="connectionBinding">The connection binding.</param>
        /// <exception cref="System.ArgumentNullException">connectionBinding</exception>
        public ConnectionBinding(ConnectionBinding connectionBinding)
        {
            if (connectionBinding == null)
            {
                throw new ArgumentNullException("connectionBinding");
            }
            _cluster = connectionBinding.Cluster;
            _node = connectionBinding.Node;
            _connectionBinding = connectionBinding;
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
            get { return _cluster; }
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">ConnectionBinding</exception>
        public MongoConnection Connection
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }
                return _connection ?? _connectionBinding.Connection;
            }
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// The node.
        /// </value>
        public MongoServerInstance Node
        {
            get { return _node; }
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
        /// Gets a binding to a connection.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>
        /// A connection binding.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">selector</exception>
        /// <exception cref="System.ObjectDisposedException">ConnectionBinding</exception>
        public ConnectionBinding GetConnectionBinding(INodeSelector selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }
            selector.EnsureCurrentNodeIsAcceptable(_node);
            return new ConnectionBinding(this); // chained binding
        }

        /// <summary>
        /// Gets the last error.
        /// </summary>
        /// <returns>A GetLastErrorResult.</returns>
        /// <exception cref="System.ObjectDisposedException">ConnectionBinding</exception>
        public GetLastErrorResult GetLastError()
        {
            if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }
            return GetLastError("admin");
        }

        /// <summary>
        /// Gets the last error.
        /// </summary>
        /// <param name="databaseName">The name of the database to send the getLastError command to.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">databaseName</exception>
        /// <exception cref="System.ObjectDisposedException">ConnectionBinding</exception>
        public GetLastErrorResult GetLastError(string databaseName)
        {
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }
            var databaseSettings = new MongoDatabaseSettings { Binding = this };
            var database = _cluster.GetDatabase(databaseName, databaseSettings);
            return database.RunCommandAs<GetLastErrorResult>("getlasterror"); // use all lowercase for backward compatibility
        }

        /// <summary>
        /// Gets a binding to a node.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>
        /// A node binding.
        /// </returns>
        public IMongoBinding GetNodeBinding(INodeSelector selector)
        {
            return GetConnectionBinding(selector);
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
