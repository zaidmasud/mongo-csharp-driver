/* Copyright 2010-2012 10gen Inc.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

using MongoDB.Driver.Internal;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a connection to a MongoServerInstance.
    /// </summary>
    public class MongoConnection : IDisposable
    {
        // private fields
        private readonly IMongoBinding _binding;
        private readonly MongoInternalConnection _internalConnection;

        private bool _disposed;

        // constructors
        internal MongoConnection(IMongoBinding binding, MongoInternalConnection internalConnection)
        {
            _binding = binding;
            _internalConnection = internalConnection;
        }

        // public properties
        /// <summary>
        /// Gets the binding this connection was acquired from.
        /// </summary>
        public IMongoBinding Binding
        {
            get { return _binding; }
        }

        /// <summary>
        /// Gets the internal connection.
        /// </summary>
        public MongoInternalConnection InternalConnection
        {
            get { return _internalConnection; }
        }

        /// <summary>
        /// Gets the server instance this connection is connected to.
        /// </summary>
        public MongoServerInstance ServerInstance
        {
            get { return _internalConnection.ServerInstance; }
        }

        // public methods
        /// <summary>
        /// Authenticate against an additional database.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <param name="credentials">The credentials.</param>
        public void Authenticate(string databaseName, MongoCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _binding.ReleaseConnection(this);
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        public IMongoBinding GetBinding()
        {
            return new MongoConnectionBinding(_internalConnection);
        }

        /// <summary>
        /// Logoff from a database.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        public void Logoff(string databaseName)
        {
            throw new NotImplementedException();
        }
    }
}
