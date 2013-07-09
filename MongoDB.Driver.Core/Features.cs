using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core
{
    /// <summary>
    /// Features useful to query <see cref="MongoDB.Driver.Core.Connections.ServerDescription"/>.
    /// </summary>
    public enum Feature
    {
        /// <summary>
        /// The aggregation framework feature.
        /// </summary>
        AggregationFramework,
        /// <summary>
        /// The aggregation framework returns a cursor feature.
        /// </summary>
        AggregationFrameworkReturnsACursor,
        /// <summary>
        /// Map reduce returns a cursor feature.
        /// </summary>
        MapReduceReturnsACursor
    }
}