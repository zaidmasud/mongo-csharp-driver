using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// Options for ISession.CreateSessionChannelProvider.
    /// </summary>
    public class CreateSessionChannelProviderOptions
    {
        // private fields
        private readonly bool _isQuery;
        private readonly IServerSelector _serverSelector;
        private CancellationToken _cancellationToken;
        private bool _disposeSession;
        private TimeSpan _timeout;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSessionChannelProviderOptions" /> class.
        /// </summary>
        /// <param name="serverSelector">The server selector.</param>
        /// <param name="isQuery">if set to <c>true</c> [is query].</param>
        public CreateSessionChannelProviderOptions(IServerSelector serverSelector, bool isQuery)
        {
            _serverSelector = serverSelector;
            _isQuery = isQuery;

            _cancellationToken = CancellationToken.None;
            _disposeSession = false;
            _timeout = TimeSpan.FromSeconds(30);
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
        /// Gets or sets a value indicating whether to dispose of the session when the IOperationChannelProvider is disposed.
        /// </summary>
        public bool DisposeSession
        {
            get { return _disposeSession; }
            set { _disposeSession = value; }
        }

        /// <summary>
        /// Gets a value indicating whether to create an IOperationChannelProvider for a query.
        /// </summary>
        public bool IsQuery
        {
            get { return _isQuery; }
        }

        /// <summary>
        /// Gets the server selector.
        /// </summary>
        public IServerSelector ServerSelector
        {
            get { return _serverSelector; }
        }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
    }
}