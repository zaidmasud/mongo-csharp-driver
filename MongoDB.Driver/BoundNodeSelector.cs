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
    /// Represents a dummy selector that is useful when you are already bound to a node or a connection.
    /// </summary>
    public class BoundNodeSelector : INodeSelector
    {
        /// <summary>
        /// Ensures that the current node is acceptable.
        /// </summary>
        /// <param name="node">The node.</param>
        public void EnsureCurrentNodeIsAcceptable(MongoServerInstance node)
        {
        }

        /// <summary>
        /// Throws an exception because the binding should already be to a node or connection.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns>
        /// Throws an exception.
        /// </returns>
        /// <exception cref="System.NotImplementedException">BoundNodeSelector can only be used with a binding to a node or a connection.</exception>
        public MongoServerInstance SelectNode(MongoServer cluster)
        {
            throw new NotImplementedException("BoundNodeSelector can only be used with a binding to a node or a connection.");
        }
    }
}
