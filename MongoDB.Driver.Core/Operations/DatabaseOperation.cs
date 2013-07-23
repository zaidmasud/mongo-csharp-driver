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
using System.Threading;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Sessions;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Base class for an operation.
    /// </summary>
    public abstract class DatabaseOperation<T> : IOperation<T>
    {
        // private fields
        private CancellationToken _cancellationToken;
        private BsonBinaryReaderSettings _readerSettings;
        private ISession _session;
        private TimeSpan _timeout;
        private BsonBinaryWriterSettings _writerSettings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseOperation{T}" /> class.
        /// </summary>
        protected DatabaseOperation()
        {
            _cancellationToken = CancellationToken.None;
            _readerSettings = BsonBinaryReaderSettings.Defaults;
            _timeout = TimeSpan.FromSeconds(30);
            _writerSettings = BsonBinaryWriterSettings.Defaults;
        }

        // public properties
        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
            set { _cancellationToken = value; }
        }

        /// <summary>
        /// Gets or sets the reader settings.
        /// </summary>
        public BsonBinaryReaderSettings ReaderSettings
        {
            get { return _readerSettings; }
            set
            {
                Ensure.IsNotNull("value", value);
                _readerSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        public ISession Session
        {
            get { return _session; }
            set { _session = value; }
        }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <summary>
        /// Gets or sets the writer settings.
        /// </summary>
        public BsonBinaryWriterSettings WriterSettings
        {
            get { return _writerSettings; }
            set
            {
                Ensure.IsNotNull("value", value);
                _writerSettings = value;
            }
        }

        // public methods
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>An operation channel provider.</returns>
        public T Execute()
        {
            return Execute(OperationBehavior.Default);
        }

        /// <summary>
        /// Executes the specified operation behavior.
        /// </summary>
        /// <param name="operationBehavior">The operation behavior.</param>
        /// <returns>An operation channel provider.</returns>
        public abstract T Execute(OperationBehavior operationBehavior);

        // protected methods
        /// <summary>
        /// Creates the session channel provider.
        /// </summary>
        /// <param name="serverSelector">The server selector.</param>
        /// <param name="isQuery">if set to <c>true</c> the operation is a query.</param>
        /// <param name="operationBehavior">The operation behavior.</param>
        /// <returns>A session channel provider.</returns>
        protected ISessionChannelProvider CreateSessionChannelProvider(IServerSelector serverSelector, bool isQuery, OperationBehavior operationBehavior)
        {
            var options = new CreateSessionChannelProviderArgs(serverSelector, isQuery)
            {
                CancellationToken = _cancellationToken,
                DisposeSession = operationBehavior.HasFlag(OperationBehavior.CloseSession),
                Timeout = _timeout
            };

            return Session.CreateSessionChannelProvider(options);
        }

        /// <summary>
        /// Adjusts the reader settings based on server specific settings.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>The adjusted reader settings</returns>
        protected BsonBinaryReaderSettings GetServerAdjustedReaderSettings(ServerDescription server)
        {
            Ensure.IsNotNull("server", server);

            var readerSettings = _readerSettings.Clone();
            readerSettings.MaxDocumentSize = server.MaxDocumentSize;
            return readerSettings;
        }

        // protected methods
        /// <summary>
        /// Adjusts the writer settings based on server specific settings.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>The adjusted writer settings.</returns>
        protected BsonBinaryWriterSettings GetServerAdjustedWriterSettings(ServerDescription server)
        {
            Ensure.IsNotNull("server", server);

            var writerSettings = _writerSettings.Clone();
            writerSettings.MaxDocumentSize = server.MaxDocumentSize;
            return writerSettings;
        }

        /// <summary>
        /// Validates the required properties.
        /// </summary>
        protected virtual void ValidateRequiredProperties()
        {
            Ensure.IsNotNull("Session", _session);
            Ensure.IsInfiniteOrZeroOrPositive("Timeout", _timeout);
        }
    }
}