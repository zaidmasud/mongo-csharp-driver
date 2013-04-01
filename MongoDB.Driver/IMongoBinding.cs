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
        // methods
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

        /// <summary>
        /// Gets a new binding that is at least bound to a node but may be more narrowly bound.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>A node binding.</returns>
        /// <remarks>
        /// If the current binding is to a cluster, the selector is used to select a node.
        /// If the current binding is to a node or connection, the selector is used to verify that the selected node is still acceptable.
        /// </remarks>
        INodeBinding NarrowToNode(INodeSelector selector);

        /// <summary>
        /// Gets a binding to a connection.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>A connection binding.</returns>
        /// <remarks>
        /// Since IConnectionBinding is IDisposable, be sure to call Dispose when you are done with the binding.
        /// 
        /// The first connection binding returned for a connection owns the connection and will release the connection
        /// back to the connection pool when Dispose is called.
        /// 
        /// If you call NarrowToConnection on a connection binding, you will get a new connection binding that does
        /// not own the connection and will not release it when Dispose is called. Not until Dispose is called for
        /// the outermost connection binding is the connection itself released.
        /// </remarks>
        IConnectionBinding NarrowToConnection(INodeSelector selector);
    }
}
