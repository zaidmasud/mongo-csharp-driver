using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// An operation against the server.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOperation<T>
    {
        /// <summary>
        /// Gets a value indicating whether this operation is a query operation.
        /// </summary>
        bool IsQuery { get; }

        /// <summary>
        /// Gets the server selector.
        /// </summary>
        IServerSelector ServerSelector { get; }

        /// <summary>
        /// Executes the specified channel provider.
        /// </summary>
        /// <param name="channelProvider">The channel provider.</param>
        /// <returns>The result of the operation.</returns>
        T Execute(IOperationChannelProvider channelProvider);
    }
}