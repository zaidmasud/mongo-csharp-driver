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

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a node in a cluster.
    /// </summary>
    public class MongoNode : IMongoBinding
    {
        // private fields
        private readonly MongoServer _cluster;
        private readonly MongoServerInstance _instance;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoNode"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="instance">The node.</param>
        /// <exception cref="System.ArgumentNullException">
        /// cluster
        /// or
        /// node
        /// </exception>
        public MongoNode(MongoServer cluster, MongoServerInstance instance)
        {
            if (cluster == null)
            {
                throw new ArgumentNullException("cluster");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            _cluster = cluster;
            _instance = instance;
        }

        // public properties
        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public MongoServerAddress Address
        {
            get { return _instance.Address; }
        }

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
        /// Gets the type of the instance.
        /// </summary>
        /// <value>
        /// The type of the instance.
        /// </value>
        public MongoServerInstanceType InstanceType
        {
            get { return _instance.InstanceType; }
        }

        /// <summary>
        /// Gets the max document size.
        /// </summary>
        /// <value>
        /// The max document size.
        /// </value>
        public int MaxDocumentSize
        {
            get { return _instance.MaxDocumentSize; }
        }

        /// <summary>
        /// Gets the max message length.
        /// </summary>
        /// <value>
        /// The max message length.
        /// </value>
        public int MaxMessageLength
        {
            get { return _instance.MaxMessageLength; }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public MongoServerState State
        {
            get { return _instance.State; }
        }

        // internal properties
        internal MongoServerInstance Instance
        {
            get { return _instance; }
        }

        // public methods
        /// <summary>
        /// Gets a connection binding.
        /// </summary>
        /// <returns>
        /// A connection binding.
        /// </returns>
        public ConnectionBinding GetConnectionBinding(ReadPreference readPreference)
        {
            ThrowIfNodeDoesNotMatchReadPreference(readPreference);
            var connection = _instance.AcquireConnection();
            return new ConnectionBinding(_cluster, this, connection);
        }

        /// <summary>
        /// Gets a database bound to this node.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>A database bound to this node.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// databaseName
        /// </exception>
        public MongoDatabase GetDatabase(string databaseName)
        {
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            return GetDatabase(databaseName, new MongoDatabaseSettings());
        }

        /// <summary>
        /// Gets a database bound to this node.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="databaseSettings">The database settings.</param>
        /// <returns>A database bound to this node.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// databaseName
        /// or
        /// databaseSettings
        /// </exception>
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
            return new MongoDatabase(this, _cluster, databaseName, databaseSettings);
        }

        /// <summary>
        /// Gets a node binding compatible with the read preference.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <returns>
        /// A node binding.
        /// </returns>
        public IMongoBinding GetNodeBinding(ReadPreference readPreference)
        {
            ThrowIfNodeDoesNotMatchReadPreference(readPreference);
            return this;
        }

        /// <summary>
        /// Throws if node does not match read preference.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <exception cref="MongoConnectionException"></exception>
        public void ThrowIfNodeDoesNotMatchReadPreference(ReadPreference readPreference)
        {
            if (!readPreference.MatchesInstance(_instance))
            {
                var message = string.Format("Node {0} does not match ReadPreference: {1}.", _instance.Address, readPreference);
                throw new MongoConnectionException(message);
            }
        }

        // explicit interface implementations
        MongoNode IMongoBinding.Node
        {
            get { return this; }
        }
    }
}
