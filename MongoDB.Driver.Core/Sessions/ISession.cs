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
        /// Creates a session channel provider.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A session channel provider.</returns>
        ISessionChannelProvider CreateSessionChannelProvider(CreateSessionChannelProviderArgs options);
    }
}