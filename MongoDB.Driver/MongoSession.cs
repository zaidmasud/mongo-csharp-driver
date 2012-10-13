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
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a session with a server instance. Call Dispose when you are done with the session.
    /// </summary>
    public class MongoSession : IDisposable
    {
        // private fields
        private readonly MongoServer _server;
        private readonly MongoServerInstance _serverInstance;
        private readonly MongoConnection _connection;
        private readonly ReadPreference _readPreference;

        private bool _disposed;

        // constructors
        internal MongoSession(MongoServer server, MongoServerInstance serverInstance, MongoConnection connection, ReadPreference readPreference)
        {
            _server = server;
            _serverInstance = serverInstance;
            _connection = connection;
            _readPreference = readPreference;
        }

        // public properties
        /// <summary>
        /// Gets the server of the session.
        /// </summary>
        public MongoServer Server
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
                return _server;
            }
        }

        /// <summary>
        /// Gets the server instance of the session.
        /// </summary>
        public MongoServerInstance ServerInstance
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
                return _serverInstance;
            }
        }

        /// <summary>
        /// Gets the connection of the session.
        /// </summary>
        public MongoConnection Connection
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
                return _connection;
            }
        }

        /// <summary>
        /// Gets the read preference of the session.
        /// </summary>
        public ReadPreference ReadPreference
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
                return _readPreference;
            }
        }

        // public methods
        /// <summary>
        /// Tests whether a database exists.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>True if the database exists.</returns>
        public virtual bool DatabaseExists(string databaseName)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var adminCredentials = _server.Settings.GetCredentials("admin");
            return DatabaseExists(databaseName, adminCredentials);
        }

        /// <summary>
        /// Tests whether a database exists.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="adminCredentials">Credentials for the admin database.</param>
        /// <returns>True if the database exists.</returns>
        public virtual bool DatabaseExists(string databaseName, MongoCredentials adminCredentials)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var databaseNames = GetDatabaseNames(adminCredentials);
            return databaseNames.Contains(databaseName);
        }

        /// <summary>
        /// Disposes of the session.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _connection.ConnectionPool.ReleaseConnection(_connection);
                _disposed = true;
            }
        }

        /// <summary>
        /// Drops a database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A <see cref="CommandResult"/>.</returns>
        public virtual CommandResult DropDatabase(string databaseName)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var database = GetDatabase(databaseName);
            var command = new CommandDocument("dropDatabase", 1);
            var result = database.RunCommand(command);
            _server.IndexCache.Reset(databaseName);
            return result;
        }

        /// <summary>
        /// Fetches the document referred to by the DBRef.
        /// </summary>
        /// <param name="dbRef">The <see cref="MongoDBRef"/> to fetch.</param>
        /// <returns>A BsonDocument (or null if the document was not found).</returns>
        public virtual BsonDocument FetchDBRef(MongoDBRef dbRef)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            return FetchDBRefAs<BsonDocument>(dbRef);
        }

        /// <summary>
        /// Fetches the document referred to by the DBRef, deserialized as a <typeparamref name="TDocument"/>.
        /// </summary>
        /// <typeparam name="TDocument">The nominal type of the document to fetch.</typeparam>
        /// <param name="dbRef">The <see cref="MongoDBRef"/> to fetch.</param>
        /// <returns>A <typeparamref name="TDocument"/> (or null if the document was not found).</returns>
        public virtual TDocument FetchDBRefAs<TDocument>(MongoDBRef dbRef)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            return (TDocument)FetchDBRefAs(typeof(TDocument), dbRef);
        }

        /// <summary>
        /// Fetches the document referred to by the DBRef.
        /// </summary>
        /// <param name="documentType">The nominal type of the document to fetch.</param>
        /// <param name="dbRef">The <see cref="MongoDBRef"/> to fetch.</param>
        /// <returns>The document (or null if the document was not found).</returns>
        public virtual object FetchDBRefAs(Type documentType, MongoDBRef dbRef)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            if (dbRef.DatabaseName == null)
            {
                throw new ArgumentException("MongoDBRef DatabaseName missing.");
            }

            var database = GetDatabase(dbRef.DatabaseName);
            return database.FetchDBRefAs(documentType, dbRef);
        }

        /// <summary>
        /// Gets a MongoDatabase instance representing a database on this server. Only one instance
        /// is created for each combination of database settings.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A new or existing instance of MongoDatabase.</returns>
        public virtual MongoDatabase GetDatabase(string databaseName)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var databaseSettings = new MongoDatabaseSettings();
            return GetDatabase(databaseName, databaseSettings);
        }

        /// <summary>
        /// Gets a MongoDatabase instance representing a database on this server. Only one instance
        /// is created for each combination of database settings.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="databaseSettings">The settings to use with this database.</param>
        /// <returns>A new or existing instance of MongoDatabase.</returns>
        public virtual MongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings databaseSettings)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (databaseSettings == null)
            {
                throw new ArgumentNullException("databaseSettings");
            }
            return new MongoDatabase(this, databaseName, databaseSettings);
        }

        /// <summary>
        /// Gets the names of the databases on this server.
        /// </summary>
        /// <returns>A list of database names.</returns>
        public virtual IEnumerable<string> GetDatabaseNames()
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var adminCredentials = _server.Settings.GetCredentials("admin");
            return GetDatabaseNames(adminCredentials);
        }

        /// <summary>
        /// Gets the names of the databases on this server.
        /// </summary>
        /// <param name="adminCredentials">Credentials for the admin database.</param>
        /// <returns>A list of database names.</returns>
        public virtual IEnumerable<string> GetDatabaseNames(MongoCredentials adminCredentials)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var adminDatabaseSettings = new MongoDatabaseSettings { Credentials = adminCredentials };
            var adminDatabase = GetDatabase("admin", adminDatabaseSettings);
            var result = adminDatabase.RunCommand("listDatabases");
            var databaseNames = new List<string>();
            foreach (BsonDocument database in result.Response["databases"].AsBsonArray.Values)
            {
                string databaseName = database["name"].AsString;
                databaseNames.Add(databaseName);
            }
            databaseNames.Sort();
            return databaseNames;
        }

        /// <summary>
        /// Gets the last error (if any) that occurred on this connection.
        /// </summary>
        /// <returns>The last error (<see cref=" GetLastErrorResult"/>)</returns>
        public virtual GetLastErrorResult GetLastError()
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var adminDatabase = GetDatabase("admin");
            return adminDatabase.GetLastError();
        }

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        public virtual void Shutdown()
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            var adminCredentials = _server.Settings.GetCredentials("admin");
            Shutdown(adminCredentials);
        }

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        /// <param name="adminCredentials">Credentials for the admin database.</param>
        public virtual void Shutdown(MongoCredentials adminCredentials)
        {
            if (_disposed) { throw new ObjectDisposedException("MongoSession"); }
            try
            {
                var adminDatabaseSettings = new MongoDatabaseSettings { Credentials = adminCredentials };
                var adminDatabase = GetDatabase("admin", adminDatabaseSettings);
                adminDatabase.RunCommand("shutdown");
            }
            catch (EndOfStreamException)
            {
                // we expect an EndOfStreamException when the server shuts down so we ignore it
            }
        }
    }
}
