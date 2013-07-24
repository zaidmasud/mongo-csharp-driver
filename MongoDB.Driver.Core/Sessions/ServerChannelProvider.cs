/* Copyright 2010-2013 10gen Inc.
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
using System.Linq;
using System.Threading;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Sessions
{
    internal sealed class ServerChannelProvider : IServerChannelProvider
    {
        private readonly ISession _session;
        private readonly IServer _server;
        private readonly bool _disposeSession;
        private bool _disposed;

        public ServerChannelProvider(ISession session, IServer server, bool disposeSession)
        {
            _session = session;
            _server = server;
            _disposeSession = disposeSession;
        }

        public ServerDescription Server
        {
            get
            {
                ThrowIfDisposed();
                return _server.Description;
            }
        }

        public IChannel GetChannel(TimeSpan timeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _server.GetChannel(timeout, cancellationToken);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_disposeSession)
                {
                    _session.Dispose();
                }
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}