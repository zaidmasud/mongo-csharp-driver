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

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a binding to a cluster, node or connection.
    /// </summary>
    public interface IMongoBinding
    {
        // properties
        /// <summary>
        /// Gets the cluster.
        /// </summary>
        /// <value>
        /// The cluster.
        /// </value>
        MongoServer Cluster { get; }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// The node (null if bound only to the cluster).
        /// </value>
        MongoNode Node { get; }

        // methods
        /// <summary>
        /// Gets a database with this binding.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>A database.</returns>
        MongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings settings);

        /// <summary>
        /// Gets a binding to a node.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>A node binding.</returns>
        IMongoBinding GetNodeBinding(INodeSelector selector);

        /// <summary>
        /// Gets a binding to a connection.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>A connection binding.</returns>
        ConnectionBinding GetConnectionBinding(INodeSelector selector);
    }
}
