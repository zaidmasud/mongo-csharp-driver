/* Copyright 2010-2012 10gen Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MongoDB.Driver
{
    /// <summary>
    /// Creates a MongoServerInstanceManager based on the settings.
    /// </summary>
    internal class MongoServerProxyFactory
    {
        // private static fields
        private static readonly MongoServerProxyFactory __instance = new MongoServerProxyFactory();

        // private fields
        private readonly object _lock = new object();
        private readonly Dictionary<MongoServerSettings, IMongoServerProxy> _serverProxies = new Dictionary<MongoServerSettings, IMongoServerProxy>();
        private int _nextSequentialId;
        private int _maxProxyCount = 100;

        // public static properties
        public static MongoServerProxyFactory Instance
        {
            get { return __instance; }
        }

        // public properties
        /// <summary>
        /// Gets or sets the maximum number of proxies that will be allowed to be created.
        /// </summary>
        public int MaxProxyCount
        {
            get { return _maxProxyCount; }
            set { _maxProxyCount = value; }
        }

        /// <summary>
        /// Gets the number of proxies that have been created.
        /// </summary>
        public int ProxyCount
        {
            get
            {
                lock (_lock)
                {
                    return _serverProxies.Count;
                }
            }
        }

        // public methods
        /// <summary>
        /// Gets an array containing a snapshot of the set of all proxies that have been created so far.
        /// </summary>
        /// <returns>An array containing a snapshot of the set of all proxies that have been created so far.</returns>
        public IMongoServerProxy[] GetAllProxies()
        {
            lock (_lock)
            {
                return _serverProxies.Values.ToArray();
            }
        }

        /// <summary>
        /// Gets a proxy for the setting (creates a new proxy if one isn't already cached).
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>A proxy.</returns>
        public IMongoServerProxy GetServerProxy(MongoServerSettings settings)
        {
            lock (_lock)
            {
                IMongoServerProxy serverProxy;
                if (!_serverProxies.TryGetValue(settings, out serverProxy))
                {
                    if (_serverProxies.Count >= _maxProxyCount)
                    {
                        var message = string.Format("MongoServerProxyFactory.GetProxy has already created {0} proxies which is the maximum number of proxies allowed.", _maxProxyCount);
                        throw new MongoException(message);
                    }
                    serverProxy = CreateServerProxy(settings);
                    _serverProxies.Add(settings, serverProxy);
                }
                return serverProxy;
            }
        }

        /// <summary>
        /// Unregisters all proxies from the dictionary used by Create to remember which proxies have already been created.
        /// </summary>
        public void UnregisterAllProxies()
        {
            lock (_lock)
            {
                var proxyList = _serverProxies.Values.ToList();
                foreach (var serverProxy in proxyList)
                {
                    UnregisterProxy(serverProxy);
                }
            }
        }

        /// <summary>
        /// Unregisters a proxy from the dictionary used to remember which proxies have already been created.
        /// </summary>
        /// <param name="serverProxy">The proxy to unregister.</param>
        public void UnregisterProxy(IMongoServerProxy serverProxy)
        {
            lock (_lock)
            {
                try { serverProxy.Disconnect(); }
                catch { } // ignore exceptions
                _serverProxies.Remove(serverProxy.Settings);
            }
        }

        // private methods
        /// <summary>
        /// Creates an IMongoServerProxy of some type that depends on the server settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>An IMongoServerProxy.</returns>
        private IMongoServerProxy CreateServerProxy(MongoServerSettings settings)
        {
            var connectionMode = settings.ConnectionMode;
            if (settings.ConnectionMode == ConnectionMode.Automatic)
            {
                if (settings.ReplicaSetName != null)
                {
                    connectionMode = ConnectionMode.ReplicaSet;
                }
                else if (settings.Servers.Count() == 1)
                {
                    connectionMode = ConnectionMode.Direct;
                }
            }

            var sequentialId = Interlocked.Increment(ref _nextSequentialId);
            switch (connectionMode)
            {
                case ConnectionMode.Direct:
                    return new DirectMongoServerProxy(settings, sequentialId);
                case ConnectionMode.ReplicaSet:
                    return new ReplicaSetMongoServerProxy(settings, sequentialId);
                case ConnectionMode.ShardRouter:
                    return new ShardedMongoServerProxy(settings, sequentialId);
                default:
                    return new DiscoveringMongoServerProxy(settings, sequentialId);
            }
        }
    }
}