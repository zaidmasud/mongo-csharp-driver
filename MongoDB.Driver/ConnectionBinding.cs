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
    public class ConnectionBinding : IConnectionBinding
    {
        // private fields
        private readonly MongoServer _cluster;
        private readonly MongoNode _node;
        private readonly MongoConnection _connection;
        private readonly ConnectionBinding _wrappedBinding;
        private bool _disposed;

        // constructors
        internal ConnectionBinding(MongoServer cluster, MongoNode node, MongoConnection connection)
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

        private ConnectionBinding(ConnectionBinding wrappedBinding)
        {
            if (wrappedBinding == null)
            {
                throw new ArgumentNullException("wrappedBinding");
            }
            _cluster = wrappedBinding.Cluster;
            _node = wrappedBinding.Node;
            _wrappedBinding = wrappedBinding;
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
                if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }
                return _cluster;
            }
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
                return _connection ?? _wrappedBinding.Connection;
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
                if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }
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
        /// Gets a binding to a connection.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>
        /// A connection binding.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">selector</exception>
        /// <exception cref="System.ObjectDisposedException">ConnectionBinding</exception>
        public IConnectionBinding NarrowToConnection(INodeSelector selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }
            selector.EnsureCurrentNodeIsAcceptable(_node);

            return new ConnectionBinding(this); // wrap this binding
        }

        /// <summary>
        /// Gets a database bound to this connection.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// databaseName
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">ConnectionBinding</exception>
        public MongoDatabase GetDatabase(string databaseName)
        {
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }

            return GetDatabase(databaseName, new MongoDatabaseSettings());
        }

        /// <summary>
        /// Gets a database bound to this connection.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="databaseSettings">The database settings.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// databaseName
        /// or
        /// databaseSettings
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">ConnectionBinding</exception>
        public MongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings databaseSettings)
        {
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (databaseSettings == null)
            {
                throw new ArgumentNullException("databaseSettings");
            }
            if (_disposed) { throw new ObjectDisposedException("ConnectionBinding"); }

            return new MongoDatabase(this, _cluster, databaseName, databaseSettings);
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

            var databaseSettings = new MongoDatabaseSettings();
            var database = GetDatabase(databaseName, databaseSettings);
            return database.RunCommandAs<GetLastErrorResult>("getlasterror"); // use all lowercase for backward compatibility
        }

        /// <summary>
        /// Gets a binding to a node.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>
        /// A node binding.
        /// </returns>
        public INodeBinding NarrowToNode(INodeSelector selector)
        {
            // keep binding focused on the connection
            return NarrowToConnection(selector);
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
