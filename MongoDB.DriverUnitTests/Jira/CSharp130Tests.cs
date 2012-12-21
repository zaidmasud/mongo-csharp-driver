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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDB.DriverUnitTests.Jira.CSharp130
{
    [TestFixture]
    public class CSharp130Tests
    {
#pragma warning disable 649 // never assigned to
        private class C
        {
            public ObjectId Id;
            public IList<int> List;
        }
#pragma warning restore

        [Test]
        public void TestLastErrorMessage()
        {
            var clientSettings = Configuration.TestClient.Settings.Clone();
            clientSettings.WriteConcern = WriteConcern.Unacknowledged;
            var client = new MongoClient(clientSettings); // WriteConcern is Unacknowledged
            var server = client.GetServer();

            using (var connection = server.GetConnection(Configuration.TestDatabase.Name))
            {
                var database = connection.GetDatabase(Configuration.TestDatabase.Name);
                var collection = database.GetCollection<C>(Configuration.TestCollection.Name);

                var c = new C { List = new List<int>() };

                // insert it once
                collection.Insert(c);
                var lastError = database.GetLastError();
                Assert.AreEqual(0, lastError.DocumentsAffected);
                Assert.IsFalse(lastError.HasLastErrorMessage);
                Assert.IsNull(lastError.LastErrorMessage);
                Assert.IsFalse(lastError.UpdatedExisting);

                // insert it again (expect duplicate key error, but no exception because WriteConcern = WriteConcern.Unacknowledged)
                collection.Insert(c);
                lastError = database.GetLastError();
                Assert.AreEqual(0, lastError.DocumentsAffected);
                Assert.IsTrue(lastError.HasLastErrorMessage);
                Assert.IsNotNull(lastError.LastErrorMessage);
                Assert.IsFalse(lastError.UpdatedExisting);
            }
        }
    }
}
