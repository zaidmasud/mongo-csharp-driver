using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Behavior options for how operations execute.
    /// </summary>
    [Flags]
    public enum OperationBehavior
    {
        /// <summary>
        /// The default operation behavior.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Indicates that the session should be closed when the operation is disposed.
        /// </summary>
        CloseSession = 1
    }
}
