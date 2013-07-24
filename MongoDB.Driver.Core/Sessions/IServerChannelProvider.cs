﻿/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Threading;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Sessions
{
    /// <summary>
    /// Represents a source of channels from a server.
    /// </summary>
    public interface IServerChannelProvider : IDisposable
    {
        /// <summary>
        /// Gets the server.
        /// </summary>
        ServerDescription Server { get; }

        /// <summary>
        /// Gets a channel.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A channel.</returns>
        IChannel GetChannel(TimeSpan timeout, CancellationToken cancellationToken);
    }
}