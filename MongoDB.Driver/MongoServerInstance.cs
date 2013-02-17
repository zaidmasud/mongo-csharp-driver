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
using System.Net;
using MongoDB.Driver.Internal;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents an instance of a MongoDB server host.
    /// </summary>
    public sealed class MongoServerInstance : IMongoBinding
    {
        // private fields
        private readonly MongoServer _server;
        private readonly MongoServerInstanceInternal _inner;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoServerInstance"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="inner">The inner server instance.</param>
        internal MongoServerInstance(MongoServer server, MongoServerInstanceInternal inner)
        {
            _server = server;
            _inner = inner;
        }

        // public properties
        /// <summary>
        /// Gets the inner MongoServerInstanceInternal.
        /// </summary>
        public MongoServerInstanceInternal Inner
        {
            get { return _inner; }
        }

        /// <summary>
        /// Gets the instance type.
        /// </summary>
        public MongoServerInstanceType InstanceType
        {
            get { return _inner.InstanceType; }
        }

        // public properties
        /// <summary>
        /// Gets the address of this server instance.
        /// </summary>
        public MongoServerAddress Address
        {
            get { return _inner.Address; }
        }

        /// <summary>
        /// Gets the version of this server instance.
        /// </summary>
        public MongoServerBuildInfo BuildInfo
        {
            get { return _inner.BuildInfo; }
        }

        /// <summary>
        /// Gets the exception thrown the last time Connect was called (null if Connect did not throw an exception).
        /// </summary>
        public Exception ConnectException
        {
            get { return _inner.ConnectException; }
        }

        /// <summary>
        /// Gets the connection pool for this server instance.
        /// </summary>
        public MongoConnectionPool ConnectionPool
        {
            get { return _inner.ConnectionPool; }
        }

        /// <summary>
        /// Gets whether this server instance is an arbiter instance.
        /// </summary>
        public bool IsArbiter
        {
            get { return _inner.IsArbiter; }
        }

        /// <summary>
        /// Gets the result of the most recent ismaster command sent to this server instance.
        /// </summary>
        public IsMasterResult IsMasterResult
        {
            get { return _inner.IsMasterResult; }
        }

        /// <summary>
        /// Gets whether this server instance is a passive instance.
        /// </summary>
        public bool IsPassive
        {
            get { return _inner.IsPassive; }
        }

        /// <summary>
        /// Gets whether this server instance is a primary.
        /// </summary>
        public bool IsPrimary
        {
            get { return _inner.IsPrimary; }
        }

        /// <summary>
        /// Gets whether this server instance is a secondary.
        /// </summary>
        public bool IsSecondary
        {
            get { return _inner.IsSecondary; }
        }

        /// <summary>
        /// Gets the max document size for this server instance.
        /// </summary>
        public int MaxDocumentSize
        {
            get { return _inner.MaxDocumentSize; }
        }

        /// <summary>
        /// Gets the max message length for this server instance.
        /// </summary>
        public int MaxMessageLength
        {
            get { return _inner.MaxMessageLength; }
        }

        /// <summary>
        /// Gets the unique sequential Id for this server instance.
        /// </summary>
        public int SequentialId
        {
            get { return _inner.SequentialId; }
        }

        /// <summary>
        /// Gets the server for this server instance.
        /// </summary>
        public MongoServer Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Gets the state of this server instance.
        /// </summary>
        public MongoServerState State
        {
            get { return _inner.State; }
        }

        // public methods
        /// <summary>
        /// Gets a MongoCollection instance bound to this server instance.
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
        /// Gets a MongoCollection instance bound to this server instance.
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
        /// Gets a MongoCollection instance bound to this server instance.
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
        /// Gets a MongoCollection instance bound to this server instance.
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
        /// Gets a connection to this server instance.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <returns>A MongoConnection.</returns>
        public MongoConnection GetConnection(ReadPreference readPreference)
        {
            // TODO: EnsureReadPreferenceIsCompatible
            var internalConnection = _inner.AcquireConnection();
            return new MongoConnection(this, _server, this, internalConnection);
        }

        /// <summary>
        /// Gets a MongoDatabase instance bound to this server instance.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A new or existing instance of MongoDatabase.</returns>
        public MongoDatabase GetDatabase(string databaseName)
        {
            var databaseSettings = new MongoDatabaseSettings();
            return GetDatabase(databaseName, databaseSettings);
        }

        /// <summary>
        /// Gets a MongoDatabase instance bound to this server instance.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="databaseSettings">The settings to use with this database.</param>
        /// <returns>A new or existing instance of MongoDatabase.</returns>
        public MongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings databaseSettings)
        {
            return new MongoDatabase(this, _server, databaseName, databaseSettings);
        }

        /// <summary>
        /// Gets the IP end point of this server instance.
        /// </summary>
        /// <returns>The IP end point of this server instance.</returns>
        public IPEndPoint GetIPEndPoint()
        {
            return _inner.GetIPEndPoint();
        }

        /// <summary>
        /// Checks whether the server is alive (throws an exception if not).
        /// </summary>
        public void Ping()
        {
            _inner.Ping();
        }

        /// <summary>
        /// Verifies the state of the server instance.
        /// </summary>
        public void VerifyState()
        {
            _inner.VerifyState();
        }

        // explicit interface implementations
        /// <summary>
        /// Releases the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        void IMongoBinding.ReleaseConnection(MongoConnectionInternal connection)
        {
            _inner.ReleaseConnection(connection);
        }
    }
}
