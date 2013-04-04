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
    /// Represents a binding to a cluster, node or connection. A binding determines
    /// where database operations are sent. A binding is ultimately combined with
    /// a ReadPreference to either determine which node to use or to verify that
    /// the node currently bound to is acceptable.
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

        // methods
        /// <summary>
        /// Applies the read preference to this binding, returning either the same binding or a new binding as necessary.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <returns>A binding matching the read preference. Either the same binding or a new one.</returns>
        INodeOrConnectionBinding ApplyReadPreference(ReadPreference readPreference);

        /// <summary>
        /// Gets a database with this binding.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A database.</returns>
        MongoDatabase GetDatabase(string databaseName);

        /// <summary>
        /// Gets a database with this binding.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>A database.</returns>
        MongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings settings);
    }
}
