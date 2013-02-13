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

using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Driver.Internal
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
        private readonly Dictionary<MongoServerProxySettings, IMongoServerProxy> _proxies = new Dictionary<MongoServerProxySettings, IMongoServerProxy>();

        private int _maxProxyCount = 100;
        private int _nextSequentialId = 1;

        // public static properties
        /// <summary>
        /// Gets the default instance.
        /// </summary>
        /// <value>
        /// The default instance.
        /// </value>
        public static MongoServerProxyFactory Instance
        {
            get { return __instance; }
        }

        // public properties
        /// <summary>
        /// Gets or sets the max proxy count.
        /// </summary>
        /// <value>
        /// The max proxy count.
        /// </value>
        public int MaxProxyCount
        {
            get
            {
                lock (_lock)
                {
                    return _maxProxyCount;
                }
            }
            set
            {
                lock (_lock)
                {
                    _maxProxyCount = value;
                }
            }
        }

        /// <summary>
        /// Gets the proxy count.
        /// </summary>
        /// <value>
        /// The proxy count.
        /// </value>
        public int ProxyCount
        {
            get
            {
                lock (_lock)
                {
                    return _proxies.Count;
                }
            }
        }

        // public methods
        /// <summary>
        /// Creates an IMongoServerProxy of some type that depends on the settings (or returns an existing one if one has already been created with these settings).
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>An IMongoServerProxy.</returns>
        public IMongoServerProxy Create(MongoServerProxySettings settings)
        {
            lock (_lock)
            {
                IMongoServerProxy proxy;
                if (!_proxies.TryGetValue(settings, out proxy))
                {
                    if (_proxies.Count >= _maxProxyCount)
                    {
                        var message = string.Format("MongoServerProxyFactory.Create has already created {0} proxies which is the maximum number of proxies allowed.", _maxProxyCount);
                        throw new MongoException(message);
                    }
                    proxy = CreateInstance(settings);
                    _proxies.Add(settings, proxy);
                }
                return proxy;
            }
        }

        /// <summary>
        /// Gets an array containing a snapshot of the set of all proxies that have been created so far.
        /// </summary>
        /// <returns>An array containing a snapshot of the set of all proxies that have been created so far.</returns>
        public IMongoServerProxy[] GetAllProxies()
        {
            lock (_lock)
            {
                return _proxies.Values.ToArray();
            }
        }

        /// <summary>
        /// Unregisters all proxies.
        /// </summary>
        public void UnregisterAllProxies()
        {
            lock (_lock)
            {
                foreach (var proxy in _proxies.Values)
                {
                    try { proxy.Disconnect(); }
                    catch { } // ignore exceptions
                }
                _proxies.Clear();
            }
        }

        /// <summary>
        /// Unregisters the proxy.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        public void UnregisterProxy(IMongoServerProxy proxy)
        {
            lock (_lock)
            {
                try { proxy.Disconnect(); }
                catch { } // ignore exceptions

                foreach (var kvp in _proxies)
                {
                    if (object.ReferenceEquals(kvp.Value, proxy))
                    {
                        _proxies.Remove(kvp.Key);
                        break;
                    }
                }
            }
        }

        // private methods
        private IMongoServerProxy CreateInstance(MongoServerProxySettings settings)
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

            var sequentialId = _nextSequentialId++;
            switch (connectionMode)
            {
                case ConnectionMode.Direct:
                    return new DirectMongoServerProxy(sequentialId, settings);
                case ConnectionMode.ReplicaSet:
                    return new ReplicaSetMongoServerProxy(sequentialId, settings);
                case ConnectionMode.ShardRouter:
                    return new ShardedMongoServerProxy(sequentialId, settings);
                default:
                    return new DiscoveringMongoServerProxy(sequentialId, settings);
            }
        }
    }
}