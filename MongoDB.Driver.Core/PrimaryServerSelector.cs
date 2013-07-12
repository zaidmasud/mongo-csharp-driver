using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core
{
    /// <summary>
    /// Selects only servers that can execute non-queries.
    /// </summary>
    public class PrimaryServerSelector : ConnectedServerSelector
    {
        // public static fields
        /// <summary>
        /// The default instance.
        /// </summary>
        public new static PrimaryServerSelector Instance = new PrimaryServerSelector();

        // private fields
        private readonly LatencyMinimizingServerSelector _latencySelector;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryServerSelector" /> class.
        /// </summary>
        private PrimaryServerSelector()
        {
            _latencySelector = new LatencyMinimizingServerSelector();
        }

        // protected methods
        /// <summary>
        /// Selects a server from the connected servers.
        /// </summary>
        /// <param name="connectedServers">The connected servers.</param>
        /// <returns>The selected server or <c>null</c> if none match.</returns>
        protected override IEnumerable<ServerDescription> SelectServerFromConnectedServers(IEnumerable<ServerDescription> connectedServers)
        {
            return _latencySelector.SelectServers(connectedServers.Where(x => x.Type.CanWrite()));
        }
    }
}