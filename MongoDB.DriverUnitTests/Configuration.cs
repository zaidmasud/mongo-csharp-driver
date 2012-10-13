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
using System.Configuration;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests
{
    /// <summary>
    /// A static class to handle online test configuration.
    /// </summary>
    public static class Configuration
    {
        // private static fields
        private static MongoServer __testServer;
        private static string __testDatabaseName;
        private static string __testCollectionName;

        // static constructor
        static Configuration()
        {
            var connectionString = Environment.GetEnvironmentVariable("CSharpDriverTestsConnectionString")
                ?? "mongodb://localhost/?safe=true";

            var mongoUrlBuilder = new MongoUrlBuilder(connectionString);
            var serverSettings = mongoUrlBuilder.ToServerSettings();
            if (!serverSettings.SafeMode.Enabled)
            {
                serverSettings.SafeMode = SafeMode.True;
            }

            __testServer = MongoServer.Create(serverSettings);
            __testDatabaseName = "csharpdriverunittests";
            __testCollectionName = "testcollection";
        }

        // public static methods
        /// <summary>
        /// Gets the name of the test collection.
        /// </summary>
        public static string TestCollectionName
        {
            get { return __testCollectionName; }
        }

        /// <summary>
        /// Gets the name of the test database.
        /// </summary>
        public static string TestDatabaseName
        {
            get { return __testDatabaseName; }
        }

        /// <summary>
        /// Gets the test server.
        /// </summary>
        public static MongoServer TestServer
        {
            get { return __testServer; }
        }
    }
}
