using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core
{
    /// <summary>
    /// Filters the servers by an allowed latency emanating from the first server.
    /// </summary>
    public class LatencyLimitingServerSelector : IServerSelector
    {
        // private fields
        private readonly TimeSpan _allowedLatency;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LatencyLimitingServerSelector" /> class.
        /// </summary>
        public LatencyLimitingServerSelector()
            : this(TimeSpan.FromMilliseconds(15))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LatencyLimitingServerSelector" /> class.
        /// </summary>
        /// <param name="allowedLatency">The allowed latency.</param>
        public LatencyLimitingServerSelector(TimeSpan allowedLatency)
        {
            Ensure.IsInfiniteOrZeroOrPositive("allowedLatency", allowedLatency);

            _allowedLatency = allowedLatency;
        }

        // public methods
        /// <summary>
        /// Selects a server from the provided servers.
        /// </summary>
        /// <param name="servers">The servers.</param>
        /// <returns>
        /// The selected server or <c>null</c> if none match.
        /// </returns>
        public IEnumerable<ServerDescription> SelectServers(IEnumerable<ServerDescription> servers)
        {
            if (_allowedLatency == Timeout.InfiniteTimeSpan)
            {
                return servers;
            }

            TimeSpan? first = null;
            return servers.OrderBy(s => s.AveragePingTime)
                .TakeWhile(s =>
                {
                    if (first.HasValue)
                    {
                        return s.AveragePingTime < first.Value.Add(_allowedLatency);
                    }
                    else
                    {
                        first = s.AveragePingTime;
                        return true;
                    }
                });
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("an allowed latency of {0}", _allowedLatency);
        }
    }
}