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
    /// Represents a binding to a node or a connection.
    /// Defines the properties and methods that node and connection bindings have in common.
    /// </summary>
    public interface INodeOrConnectionBinding : IMongoBinding
    {
        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// The node.
        /// </value>
        MongoNode Node { get; }

        // methods
        /// <summary>
        /// Gets a connection.
        /// </summary>
        /// <returns>A connection.</returns>
        ConnectionWrapper GetConnection();

        /// <summary>
        /// Gets a connection binding.
        /// </summary>
        /// <returns>A connection binding.</returns>
        ConnectionBinding GetConnectionBinding();
    }
}
