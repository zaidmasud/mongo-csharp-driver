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
using System.Threading;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Sessions
{
    internal sealed class ChannelChannelProvider : IServerChannelProvider
    {
        private readonly ISession _session;
        private readonly IChannel _channel;
        private readonly IServer _server;
        private bool _disposeSession;
        private bool _disposed;

        public ChannelChannelProvider(ISession session, IServer server, IChannel channel, bool disposeSession)
        {
            _session = session;
            _server = server;
            _channel = channel;
            _disposeSession = disposeSession;
        }

        public ServerDescription Server
        {
            get { return _server.Description; }
        }

        public IChannel GetChannel(TimeSpan timeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return new DisposalProtectedChannel(_channel);
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