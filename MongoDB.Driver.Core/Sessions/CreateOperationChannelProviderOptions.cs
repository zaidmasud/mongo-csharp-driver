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
    /// Options for ISession.CreateOperationChannelProvider.
    /// </summary>
    public class CreateOperationChannelProviderOptions
    {
        // private fields
        private readonly IServerSelector _serverSelector;
        private readonly bool _isQuery;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOperationChannelProviderOptions" /> class.
        /// </summary>
        /// <param name="serverSelector">The server selector.</param>
        /// <param name="isQuery">if set to <c>true</c> [is query].</param>
        public CreateOperationChannelProviderOptions(IServerSelector serverSelector, bool isQuery)
        {
            _serverSelector = serverSelector;
            _isQuery = isQuery;
            CloseSession = false;
            SelectServerTimeout = Timeout.InfiniteTimeSpan;
            SelectServerCancellationToken = CancellationToken.None;
        }

        // public properties
        /// <summary>
        /// Gets the server selector.
        /// </summary>
        public IServerSelector ServerSelector
        {
            get { return _serverSelector; }
        }

        /// <summary>
        /// Gets a value indicating whether to create an IOperationChannelProvider for a query.
        /// </summary>
        public bool IsQuery
        {
            get { return _isQuery; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to close the session when the IOperationChannelProvider is closed.
        /// </summary>
        public bool CloseSession { get; set; }

        /// <summary>
        /// Gets or sets the timeout to apply when selecting a server.
        /// </summary>
        public TimeSpan SelectServerTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token to use when selecting a server.
        /// </summary>
        public CancellationToken SelectServerCancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the timeout to apply when getting a channel.
        /// </summary>
        public TimeSpan GetChannelTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token to use when getting a channel.
        /// </summary>
        public CancellationToken GetChannelCancellationToken { get; set; }
    }
}