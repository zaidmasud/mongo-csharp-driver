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
    /// Represents a node selector that selects a node matching a ReadPreference.
    /// </summary>
    public class ReadPreferenceNodeSelector : INodeSelector
    {
        // private fields
        private readonly ReadPreference _readPreference;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadPreferenceNodeSelector"/> class.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <exception cref="System.ArgumentNullException">readPreference</exception>
        public ReadPreferenceNodeSelector(ReadPreference readPreference)
        {
            if (readPreference == null)
            {
                throw new ArgumentNullException("readPreference");
            }

            _readPreference = readPreference;
        }

        // public methods
        /// <summary>
        /// Ensures that the current node matches the ReadPreference.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <exception cref="System.ArgumentNullException">node</exception>
        /// <exception cref="MongoConnectionException"></exception>
        public void EnsureCurrentNodeIsAcceptable(MongoServerInstance node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            
            if (!_readPreference.MatchesInstance(node))
            {
                var message = string.Format("Node {0} no longer matches ReadPreference: {1}.", node.Address, _readPreference);
                throw new MongoConnectionException(message);
            }
        }

        /// <summary>
        /// Selects a node that matches the ReadPreference.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns>
        /// A node.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">cluster</exception>
        public MongoServerInstance SelectNode(MongoServer cluster)
        {
            if (cluster == null)
            {
                throw new ArgumentNullException("cluster");
            }

            return cluster.ChooseServerInstance(_readPreference);
        }
    }
}
