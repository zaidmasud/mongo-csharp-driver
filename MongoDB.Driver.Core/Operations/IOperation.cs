using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Sessions;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// An operation against the server.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOperation<T>
    {
        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        T Execute();

        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="operationBehavior">The operation behavior.</param>
        /// <returns>The result of the operation.</returns>
        T Execute(OperationBehavior operationBehavior);
    }
}