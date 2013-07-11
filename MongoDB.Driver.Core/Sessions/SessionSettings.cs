using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// Settings for a session.
    /// </summary>
    public class SessionSettings
    {
        // private fields
        private CancellationToken _cancellationToken;
        private TimeSpan _timeout;

        // constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionSettings" /> class.
        /// </summary>
        public SessionSettings()
        {
            _cancellationToken = CancellationToken.None;
            _timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        /// The cancellation token.
        /// </value>
        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
            set { _cancellationToken = value; }
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