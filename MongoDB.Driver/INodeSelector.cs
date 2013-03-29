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
    /// Represents an algorithm for selecting a node in a cluster (see PrimaryNodeSelector and ReadPreferenceNodeSelector).
    /// </summary>
    public interface INodeSelector
    {
        /// <summary>
        /// Ensures that the current node is acceptable.
        /// </summary>
        /// <param name="node">The node.</param>
        void EnsureCurrentNodeIsAcceptable(MongoServerInstance node);

        /// <summary>
        /// Selects the node.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns>A node.</returns>
        MongoServerInstance SelectNode(MongoServer cluster);
    }
}
