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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Internal
{
    /// <summary>
    /// Represents the state of a connection.
    /// </summary>
    public enum MongoConnectionState
    {
        /// <summary>
        /// The connection has not yet been initialized.
        /// </summary>
        Initial,
        /// <summary>
        /// The connection is open.
        /// </summary>
        Open,
        /// <summary>
        /// The connection is closed.
        /// </summary>
        Closed
    }

    /// <summary>
    /// Represents a connection to a MongoServerInstance.
    /// </summary>
    public class MongoConnection
    {
        // private static fields
        private static readonly TraceSource __trace = TraceSources.CreateGeneralTraceSource();
        private static readonly TraceSource __traceData = TraceSources.CreateDataTraceSource();
        private static int __nextSequentialId;

        // private fields
        private object _connectionLock = new object();
        private MongoServerInstance _serverInstance;
        private MongoConnectionPool _connectionPool;
        private int _generationId; // the generationId of the connection pool at the time this connection was created
        private MongoConnectionState _state;
        private TcpClient _tcpClient;
        private DateTime _createdAt;
        private DateTime _lastUsedAt; // set every time the connection is Released
        private int _messageCounter;
        private int _requestId;
        private int _sequentialId;
        private Dictionary<string, Authentication> _authentications = new Dictionary<string, Authentication>();

        // constructors
        internal MongoConnection(MongoConnectionPool connectionPool)
        {
            _serverInstance = connectionPool.ServerInstance;
            _connectionPool = connectionPool;
            _generationId = connectionPool.GenerationId;
            _createdAt = DateTime.UtcNow;
            _state = MongoConnectionState.Initial;
            _sequentialId = Interlocked.Increment(ref __nextSequentialId);
        }

        internal MongoConnection(MongoServerInstance serverInstance)
        {
            _serverInstance = serverInstance;
            _createdAt = DateTime.UtcNow;
            _state = MongoConnectionState.Initial;
            _sequentialId = Interlocked.Increment(ref __nextSequentialId);
        }

        // public properties
        /// <summary>
        /// Gets the connection pool that this connection belongs to.
        /// </summary>
        public MongoConnectionPool ConnectionPool
        {
            get { return _connectionPool; }
        }

        /// <summary>
        /// Gets the DateTime that this connection was created at.
        /// </summary>
        public DateTime CreatedAt
        {
            get { return _createdAt; }
        }

        /// <summary>
        /// Gets the generation of the connection pool that this connection belongs to.
        /// </summary>
        public int GenerationId
        {
            get { return _generationId; }
        }

        /// <summary>
        /// Gets the DateTime that this connection was last used at.
        /// </summary>
        public DateTime LastUsedAt
        {
            get { return _lastUsedAt; }
            internal set { _lastUsedAt = value; }
        }

        /// <summary>
        /// Gets a count of the number of messages that have been sent using this connection.
        /// </summary>
        public int MessageCounter
        {
            get { return _messageCounter; }
        }

        /// <summary>
        /// Gets the RequestId of the last message sent on this connection.
        /// </summary>
        public int RequestId
        {
            get { return _requestId; }
        }

        /// <summary>
        /// Gets the sequential id.
        /// </summary>
        public int SequentialId
        {
            get { return _sequentialId; }
        }

        /// <summary>
        /// Gets the server instance this connection is connected to.
        /// </summary>
        public MongoServerInstance ServerInstance
        {
            get { return _serverInstance; }
        }

        /// <summary>
        /// Gets the state of this connection.
        /// </summary>
        public MongoConnectionState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("MongoConnection[{0},{1}]", _sequentialId, _serverInstance.Address);
        }

        // internal methods
        internal bool CanAuthenticate(MongoDatabase database)
        {
            // check whether the connection can be used with the given database (and credentials)
            // the following are the only valid authentication states for a connection:
            // 1. the connection is not authenticated against any database
            // 2. the connection has a single authentication against the admin database (with a particular set of credentials)
            // 3. the connection has one or more authentications against any databases other than admin
            //    (with the restriction that a particular database can only be authenticated against once and therefore with only one set of credentials)
            // assume that IsAuthenticated was called first and returned false

            EnsureOpenConnection();
            if (database == null)
            {
                return true;
            }

            if (_authentications.Count == 0)
            {
                // a connection with no existing authentications can authenticate anything
                return true;
            }
            else
            {
                // a connection with existing authentications can't be used without credentials
                if (database.Credentials == null)
                {
                    return false;
                }

                // a connection with existing authentications can't be used with new admin credentials
                if (database.Credentials.Admin)
                {
                    return false;
                }

                // a connection with an existing authentication to the admin database can't be used with any other credentials
                if (_authentications.ContainsKey("admin"))
                {
                    return false;
                }

                // a connection with an existing authentication to a database can't authenticate for the same database again
                if (_authentications.ContainsKey(database.Name))
                {
                    return false;
                }

                return true;
            }
        }

        internal void CheckAuthentication(MongoDatabase database)
        {
            EnsureOpenConnection();
            if (database.Credentials == null)
            {
                if (_authentications.Count != 0)
                {
                    var ex = new InvalidOperationException("Connection requires credentials.");
                    __trace.TraceException(TraceEventType.Error, ex);
                    throw ex;
                }
            }
            else
            {
                var credentials = database.Credentials;
                var authenticationDatabaseName = credentials.Admin ? "admin" : database.Name;
                Authentication authentication;
                if (_authentications.TryGetValue(authenticationDatabaseName, out authentication))
                {
                    if (authentication.Credentials != database.Credentials)
                    {
                        // this shouldn't happen because a connection would have been chosen from the connection pool only if it was viable
                        if (authenticationDatabaseName == "admin")
                        {
                            var ex = new MongoInternalException("Connection already authenticated to the admin database with different credentials.");
                            __trace.TraceException(TraceEventType.Error, ex);
                            throw ex;
                        }
                        else
                        {
                            var ex = new MongoInternalException("Connection already authenticated to the database with different credentials.");
                            __trace.TraceException(TraceEventType.Error, ex);
                            throw ex;
                        }
                    }
                    authentication.LastUsed = DateTime.UtcNow;
                }
                else
                {
                    if (authenticationDatabaseName == "admin" && _authentications.Count != 0)
                    {
                        // this shouldn't happen because a connection would have been chosen from the connection pool only if it was viable
                        var ex = new MongoInternalException("The connection cannot be authenticated against the admin database because it is already authenticated against other databases.");
                        __trace.TraceException(TraceEventType.Error, ex);
                        throw ex;
                    }
                    Authenticate(authenticationDatabaseName, database.Credentials);
                }
            }
        }

        internal void Close()
        {
            __trace.TraceVerbose("{0}::closing.", this);
            lock (_connectionLock)
            {
                if (_state != MongoConnectionState.Closed)
                {
                    if (_tcpClient != null)
                    {
                        if (_tcpClient.Connected)
                        {
                            // even though MSDN says TcpClient.Close doesn't close the underlying socket
                            // it actually does (as proven by disassembling TcpClient and by experimentation)
                            try { _tcpClient.Close(); }
                            catch { } // ignore exceptions
                        }
                        _tcpClient = null;
                    }
                    _state = MongoConnectionState.Closed;
                }
            }
        }

        internal bool IsAuthenticated(MongoDatabase database)
        {
            EnsureOpenConnection();
            if (database == null)
            {
                return true;
            }

            lock (_connectionLock)
            {
                if (database.Credentials == null)
                {
                    return _authentications.Count == 0;
                }
                else
                {
                    var authenticationDatabaseName = database.Credentials.Admin ? "admin" : database.Name;
                    Authentication authentication;
                    if (_authentications.TryGetValue(authenticationDatabaseName, out authentication))
                    {
                        return database.Credentials == authentication.Credentials;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        internal void Open()
        {
            __trace.TraceVerbose("{0}::opening.", this);

            if (_state != MongoConnectionState.Initial)
            {
                var ex = new InvalidOperationException("Open called more than once.");
                __trace.TraceException(TraceEventType.Error, ex);
                throw ex;
            }

            var ipEndPoint = _serverInstance.GetIPEndPoint();
            var tcpClient = new TcpClient(ipEndPoint.AddressFamily);
            tcpClient.NoDelay = true; // turn off Nagle
            tcpClient.ReceiveBufferSize = MongoDefaults.TcpReceiveBufferSize;
            tcpClient.SendBufferSize = MongoDefaults.TcpSendBufferSize;
            tcpClient.Connect(ipEndPoint);

            _tcpClient = tcpClient;
            _state = MongoConnectionState.Open;
        }

        // this is a low level method that doesn't require a MongoServer
        // so it can be used while connecting to a MongoServer
        internal CommandResult RunCommand(
            string collectionName,
            QueryFlags queryFlags,
            CommandDocument command,
            bool throwOnError)
        {
            var commandName = command.GetElement(0).Name;

            var writerSettings = new BsonBinaryWriterSettings
            {
                GuidRepresentation = GuidRepresentation.Unspecified,
                MaxDocumentSize = _serverInstance.MaxDocumentSize
            };
            using (var message = new MongoQueryMessage(writerSettings, collectionName, queryFlags, 0, 1, command, null))
            {
                if (__traceData.Switch.ShouldTrace(TraceEventType.Information))
                {
                    __traceData.TraceInformation("{0}::command with collectionName({1}), queryFlags({2}), and command({3})", this, collectionName, queryFlags, command.ToJson());
                }
                SendMessage(message, SafeMode.False);
            }

            var readerSettings = new BsonBinaryReaderSettings
            {
                GuidRepresentation = GuidRepresentation.Unspecified,
                MaxDocumentSize = _serverInstance.MaxDocumentSize
            };
            var reply = ReceiveMessage<BsonDocument>(readerSettings, null);
            if (reply.NumberReturned == 0)
            {
                var message = string.Format("Command '{0}' failed. No response returned.", commandName);
                var ex = new MongoCommandException(message);
                __trace.TraceException(TraceEventType.Error, ex);
                throw ex;
            }

            var commandResult = new CommandResult(command, reply.Documents[0]);
            if (__traceData.Switch.ShouldTrace(TraceEventType.Verbose))
            {
                __traceData.TraceVerbose("{0}::received {1}", this, commandResult.Response.ToJson());
            }
            if (!commandResult.Ok)
            {
                var ex = new MongoCommandException(commandResult);
                __trace.TraceException(TraceEventType.Error, ex);
                if (throwOnError)
                {
                    throw ex;
                }
            }

            return commandResult;
        }

        internal MongoReplyMessage<TDocument> ReceiveMessage<TDocument>(
            BsonBinaryReaderSettings readerSettings,
            IBsonSerializationOptions serializationOptions)
        {
            EnsureOpenConnection();
            lock (_connectionLock)
            {
                try
                {
                    using (var buffer = new BsonBuffer())
                    {
                        var networkStream = GetNetworkStream();
                        var readTimeout = (int)_serverInstance.Server.Settings.SocketTimeout.TotalMilliseconds;
                        if (readTimeout != 0)
                        {
                            networkStream.ReadTimeout = readTimeout;
                        }
                        buffer.LoadFrom(networkStream);
                        var reply = new MongoReplyMessage<TDocument>(readerSettings);
                        reply.ReadFrom(buffer, serializationOptions);
                        return reply;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    throw;
                }
            }
        }

        internal SafeModeResult SendMessage(MongoRequestMessage message, SafeMode safeMode)
        {
            EnsureOpenConnection();
            lock (_connectionLock)
            {
                _requestId = message.RequestId;

                message.WriteToBuffer();
                CommandDocument safeModeCommand = null;
                if (safeMode.Enabled)
                {
                    __trace.TraceVerbose("{0}::including getLastError command because safeMode is enabled.", this);
                    safeModeCommand = new CommandDocument
                    {
                        { "getlasterror", 1 }, // use all lowercase for backward compatibility
                        { "fsync", true, safeMode.FSync },
                        { "j", true, safeMode.J },
                        { "w", safeMode.W, safeMode.W > 1 },
                        { "w", safeMode.WMode, safeMode.WMode != null },
                        { "wtimeout", (int) safeMode.WTimeout.TotalMilliseconds, safeMode.W > 1 && safeMode.WTimeout != TimeSpan.Zero }
                    };
                    if (__traceData.Switch.ShouldTrace(TraceEventType.Information))
                    {
                        __traceData.TraceInformation("{0}::sending {1}", this, safeModeCommand.ToJson());
                    }
                    // piggy back on network transmission for message
                    using (var getLastErrorMessage = new MongoQueryMessage(message.Buffer, message.WriterSettings, "admin.$cmd", QueryFlags.None, 0, 1, safeModeCommand, null))
                    {
                        getLastErrorMessage.WriteToBuffer();
                    }
                }

                try
                {
                    var networkStream = GetNetworkStream();
                    var writeTimeout = (int)_serverInstance.Server.Settings.SocketTimeout.TotalMilliseconds;
                    if (writeTimeout != 0)
                    {
                        networkStream.WriteTimeout = writeTimeout;
                    }
                    message.Buffer.WriteTo(networkStream);
                    _messageCounter++;
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    throw;
                }

                SafeModeResult safeModeResult = null;
                if (safeMode.Enabled)
                {
                    var readerSettings = new BsonBinaryReaderSettings
                    {
                        GuidRepresentation = message.WriterSettings.GuidRepresentation,
                        MaxDocumentSize = _serverInstance.MaxDocumentSize
                    };
                    var replyMessage = ReceiveMessage<BsonDocument>(readerSettings, null);
                    var safeModeResponse = replyMessage.Documents[0];
                    safeModeResult = new SafeModeResult();
                    safeModeResult.Initialize(safeModeCommand, safeModeResponse);
                    if (__traceData.Switch.ShouldTrace(TraceEventType.Verbose))
                    {
                        __traceData.TraceVerbose("{0}::received {1}", this, safeModeResult.Response.ToJson());
                    }

                    if (!safeModeResult.Ok)
                    {
                        var errorMessage = string.Format(
                            "Safemode detected an error '{0}'. (response was {1}).",
                            safeModeResult.ErrorMessage, safeModeResponse.ToJson());
                        var ex = new MongoSafeModeException(errorMessage, safeModeResult);
                        __trace.TraceException(TraceEventType.Error, ex);
                        throw ex;
                    }
                    if (safeModeResult.HasLastErrorMessage)
                    {
                        var errorMessage = string.Format(
                            "Safemode detected an error '{0}'. (Response was {1}).",
                            safeModeResult.LastErrorMessage, safeModeResponse.ToJson());
                        var ex = new MongoSafeModeException(errorMessage, safeModeResult);
                        __trace.TraceException(TraceEventType.Error, ex);
                        throw ex;
                    }
                }

                return safeModeResult;
            }
        }

        // private methods
        private void Authenticate(string databaseName, MongoCredentials credentials)
        {
            __trace.TraceVerbose("{0}::authenticating.", this);
            EnsureOpenConnection();
            lock (_connectionLock)
            {
                var nonceCommand = new CommandDocument("getnonce", 1);
                var commandCollectionName = string.Format("{0}.$cmd", databaseName);

                var commandResult = RunCommand(commandCollectionName, QueryFlags.None, nonceCommand, false);
                if (!commandResult.Ok)
                {
                    var ex = new MongoAuthenticationException(
                        "Error getting nonce for authentication.",
                        new MongoCommandException(commandResult));
                    __trace.TraceException(TraceEventType.Error, ex);
                    throw ex;
                }

                var nonce = commandResult.Response["nonce"].AsString;
                var passwordDigest = MongoUtils.Hash(credentials.Username + ":mongo:" + credentials.Password);
                var digest = MongoUtils.Hash(nonce + credentials.Username + passwordDigest);
                var authenticateCommand = new CommandDocument
                {
                    { "authenticate", 1 },
                    { "user", credentials.Username },
                    { "nonce", nonce },
                    { "key", digest }
                };

                commandResult = RunCommand(commandCollectionName, QueryFlags.None, authenticateCommand, false);
                if (__traceData.Switch.ShouldTrace(TraceEventType.Verbose))
                {
                    __traceData.TraceVerbose("{0}::received {1}", this, commandResult.Response.ToJson());
                }
                if (!commandResult.Ok)
                {
                    var message = string.Format("Invalid credentials for database '{0}'.", databaseName);
                    var ex = new MongoAuthenticationException(
                        message,
                        new MongoCommandException(commandResult));
                    __trace.TraceException(TraceEventType.Error, ex);
                    throw ex;
                }

                var authentication = new Authentication(credentials);
                _authentications.Add(databaseName, authentication);
            }
        }

        private void EnsureOpenConnection()
        {
            if (_state == MongoConnectionState.Closed)
            {
                var ex = new InvalidOperationException("Connection is closed.");
                __trace.TraceException(TraceEventType.Error, ex);
                throw ex;
            }
        }

        private NetworkStream GetNetworkStream()
        {
            if (_state == MongoConnectionState.Initial)
            {
                Open();
            }
            return _tcpClient.GetStream();
        }

        private void HandleException(Exception ex)
        {
            // there are three possible situations:
            // 1. we can keep using the connection
            // 2. just this one connection needs to be closed
            // 3. the whole connection pool needs to be cleared

            switch (DetermineAction(ex))
            {
                case HandleExceptionAction.KeepConnection:
                    break;
                case HandleExceptionAction.CloseConnection:
                    Close();
                    break;
                case HandleExceptionAction.ClearConnectionPool:
                    Close();
                    if (_connectionPool != null)
                    {
                        _connectionPool.Clear();
                    }
                    break;
                default:
                    throw new MongoInternalException("Invalid HandleExceptionAction");
            }

            // forces a call to VerifyState before the next message is sent to this server instance
            // this is a bit drastic but at least it's safe (and perhaps we can optimize a bit in the future)
            _serverInstance.SetState(MongoServerState.Unknown);
        }

        private enum HandleExceptionAction
        {
            KeepConnection,
            CloseConnection,
            ClearConnectionPool
        }

        private HandleExceptionAction DetermineAction(Exception ex)
        {
            // TODO: figure out when to return KeepConnection or ClearConnectionPool (if ever)

            // don't return ClearConnectionPool unless you are *sure* it is the right action
            // definitely don't make ClearConnectionPool the default action
            // returning ClearConnectionPool frequently can result in Connect/Disconnect storms

            return HandleExceptionAction.CloseConnection; // this should always be the default action
        }

        // private nested classes
        // keeps track of what credentials were used with a given database
        // and when that database was last used on this connection
        private class Authentication
        {
            // private fields
            private MongoCredentials _credentials;
            private DateTime _lastUsed;

            // constructors
            public Authentication(MongoCredentials credentials)
            {
                _credentials = credentials;
                _lastUsed = DateTime.UtcNow;
            }

            public MongoCredentials Credentials
            {
                get { return _credentials; }
            }

            public DateTime LastUsed
            {
                get { return _lastUsed; }
                set { _lastUsed = value; }
            }
        }
    }
}
