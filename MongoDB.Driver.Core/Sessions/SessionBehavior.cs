using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// Describes how a session should handle queries and non-queries.
    /// </summary>
    public enum SessionBehavior
    {
        /// <summary>
        /// The default session behavior.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Queries are always directed to the specified location.
        /// </summary>
        EventuallyConsistent = 0,
        /// <summary>
        /// Queries are directed to their specified location until a non-query occurs, 
        /// at which point queries are directed at the same place as non-queries.
        /// </summary>
        Monotonic = 1
    }
}