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
    /// Represents a binding to a cluster.
    /// </summary>
    public class ClusterBinding : IMongoBinding
    {
        // private fields
        private readonly MongoServer _cluster;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterBinding"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <exception cref="System.ArgumentNullException">cluster</exception>
        public ClusterBinding(MongoServer cluster)
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
            get { return _cluster; }
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// Returns null (the binding is only to a cluster).
        /// </value>
        public MongoServerInstance Node
        {
            get { return null; }
        }

        // public methods
        /// <summary>
        /// Gets a binding to a connection.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>
        /// A connection binding.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">selector</exception>
        public ConnectionBinding GetConnectionBinding(INodeSelector selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            var node = selector.SelectNode(_cluster);
            var connection = node.AcquireConnection();
            return new ConnectionBinding(_cluster, node, connection);
        }

        /// <summary>
        /// Gets a binding to a node.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>
        /// A node binding.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">selector</exception>
        public IMongoBinding GetNodeBinding(INodeSelector selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            var node = selector.SelectNode(_cluster);
            return new NodeBinding(_cluster, node);
        }
    }
}
