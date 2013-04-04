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
    /// Represents a binding that can start reading from a secondary but moves to the primary as soon as a write occurs.
    /// </summary>
    public class MonotonicReadsBinding : IMongoBinding
    {
        // private fields
        private readonly MongoServer _cluster;
        private MongoNode _node; // can move from secondary to primary
        private bool _boundToPrimary; // once we bind to the primary we won't move away from it

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MonotonicReadsBinding"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <exception cref="System.ArgumentNullException">cluster</exception>
        public MonotonicReadsBinding(MongoServer cluster)
        {
            if (cluster == null)
            {
                throw new ArgumentNullException("cluster");
            }

            _cluster = cluster;
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
                return _cluster;
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
            get { return _node; }
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
            var node = GetNodeBinding(readPreference);
            return node.GetConnectionBinding(readPreference);
        }

        /// <summary>
        /// Gets a database bound to this connection.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>A database.</returns>
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

            return GetDatabase(databaseName, new MongoDatabaseSettings());
        }

        /// <summary>
        /// Gets a database bound to this connection.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="databaseSettings">The database settings.</param>
        /// <returns>A database.</returns>
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
            if (_node == null)
            {
                // initial binding will be to either a secondary or the primary depending on the read preference
                _node = _cluster.GetNode(readPreference);
            }
            else
            {
                // check if we need to rebind to the primary
                if (readPreference.ReadPreferenceMode == ReadPreferenceMode.Primary && !_boundToPrimary)
                {
                    _node = _cluster.GetNode(ReadPreference.Primary);
                    _boundToPrimary = true; // locks in the binding to the primary
                }

                _node.ThrowIfNodeDoesNotMatchReadPreference(readPreference);
            }

            return _node;
        }
    }
}
