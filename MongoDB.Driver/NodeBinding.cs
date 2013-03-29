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
    /// Represents a binding to a node.
    /// </summary>
    public class NodeBinding : IMongoBinding
    {
        // private fields
        private readonly MongoServer _cluster;
        private readonly MongoServerInstance _node;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeBinding"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="node">The node.</param>
        /// <exception cref="System.ArgumentNullException">
        /// cluster
        /// or
        /// node
        /// </exception>
        public NodeBinding(MongoServer cluster, MongoServerInstance node)
        {
            if (cluster == null)
            {
                throw new ArgumentNullException("cluster");
            }
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            _cluster = cluster;
            _node = node;
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
        /// The node.
        /// </value>
        public MongoServerInstance Node
        {
            get { return _node; }
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

            selector.EnsureCurrentNodeIsAcceptable(_node);
            var connection = _node.AcquireConnection();
            return new ConnectionBinding(_cluster, _node, connection);
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

            selector.EnsureCurrentNodeIsAcceptable(_node);
            return this;
        }
    }
}
