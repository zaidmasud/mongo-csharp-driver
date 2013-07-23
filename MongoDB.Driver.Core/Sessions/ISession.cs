using System;
using System.Threading;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// A session decides on where operations get executed.
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Creates a server channel provider.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>A server channel provider.</returns>
        IServerChannelProvider CreateServerChannelProvider(CreateServerChannelProviderArgs args);
    }
}