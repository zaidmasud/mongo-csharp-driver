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
using MongoDB.Driver.Internal;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a connection to a MongoServerInstance.
    /// </summary>
    public class MongoConnection : IDisposable, IMongoBinding
    {
        // private fields
        private readonly IMongoBinding _sourceBinding;
        private readonly MongoServer _server;
        private readonly MongoServerInstance _serverInstance;
        private readonly MongoConnectionInternal _internalConnection;

        private bool _disposed;

        // constructors
        internal MongoConnection(IMongoBinding sourceBinding, MongoServer server, MongoServerInstance serverInstance, MongoConnectionInternal internalConnection)
        {
            _sourceBinding = sourceBinding;
            _server = server;
            _serverInstance = serverInstance;
            _internalConnection = internalConnection;
        }

        // public properties
        /// <summary>
        /// Gets the inner internal connection.
        /// </summary>
        public MongoConnectionInternal Inner
        {
            get { return _internalConnection; }
        }

        /// <summary>
        /// Gets the server instance this connection is connected to.
        /// </summary>
        public MongoServerInstance ServerInstance
        {
            get { return _serverInstance; }
        }

        // public methods
        /// <summary>
        /// Disposes of the connection by calling ReleaseConnection in the source binding.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _sourceBinding.ReleaseConnection(_internalConnection);
                }
                catch (Exception)
                {
                    // ignore exceptions
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Gets a MongoCollection instance bound to this connection.
        /// </summary>
        /// <typeparam name="TDefaultDocument">The default document type for this collection.</typeparam>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        public MongoCollection<TDefaultDocument> GetCollection<TDefaultDocument>(
            string databaseName,
            string collectionName)
        {
            return GetCollection<TDefaultDocument>(databaseName, collectionName, new MongoCollectionSettings());
        }

        /// <summary>
        /// Gets a MongoCollection instance bound to this connection.
        /// </summary>
        /// <typeparam name="TDefaultDocument">The default document type for this collection.</typeparam>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="collectionSettings">The settings to use when accessing this collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        public MongoCollection<TDefaultDocument> GetCollection<TDefaultDocument>(
            string databaseName,
            string collectionName,
            MongoCollectionSettings collectionSettings)
        {
            var database = GetDatabase(databaseName);
            return database.GetCollection<TDefaultDocument>(collectionName, collectionSettings);
        }

        /// <summary>
        /// Gets a MongoCollection instance bound to this binding.
        /// </summary>
        /// <param name="defaultDocumentType">The default document type.</param>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        public MongoCollection GetCollection(
            Type defaultDocumentType,
            string databaseName,
            string collectionName)
        {
            return GetCollection(defaultDocumentType, databaseName, collectionName, new MongoCollectionSettings());
        }

        /// <summary>
        /// Gets a MongoCollection instance bound to this binding.
        /// </summary>
        /// <param name="defaultDocumentType">The default document type.</param>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="collectionSettings">The settings to use when accessing this collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        public MongoCollection GetCollection(
            Type defaultDocumentType,
            string databaseName,
            string collectionName,
            MongoCollectionSettings collectionSettings)
        {
            var database = GetDatabase(databaseName);
            return database.GetCollection(defaultDocumentType, collectionName, collectionSettings);
        }

        /// <summary>
        /// Gets a MongoDatabase instance bound to this connection.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A MongoDatabase.</returns>
        public MongoDatabase GetDatabase(string databaseName)
        {
            return GetDatabase(databaseName, new MongoDatabaseSettings());
        }

        /// <summary>
        /// Gets a MongoDatabase instance bound to this connection.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="databaseSettings">The settings to use with this database.</param>
        /// <returns>A MongoDatabase.</returns>
        public MongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings databaseSettings)
        {
            return new MongoDatabase(this, _server, databaseName, databaseSettings);
        }

        // explicit interface implementations
        MongoConnection IMongoBinding.GetConnection(ReadPreference readPreference)
        {
            return new MongoConnection(this, _server, _serverInstance, _internalConnection);
        }

        void IMongoBinding.ReleaseConnection(MongoConnectionInternal internalConnection)
        {
            // do nothing
        }
    }
}
