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
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests.Jira
{
    [TestFixture]
    public class CSharp281Tests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                collection.Drop();
            }
        }

        [Test]
        public void TestPopFirst()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);

                var document = new BsonDocument("x", new BsonArray { 1, 2, 3 });
                collection.RemoveAll();
                collection.Insert(document);

                var query = Query.EQ("_id", document["_id"]);
                var update = Update.PopFirst("x");
                collection.Update(query, update);

                document = collection.FindOne();
                var array = document["x"].AsBsonArray;
                Assert.AreEqual(2, array.Count);
                Assert.AreEqual(2, array[0].AsInt32);
                Assert.AreEqual(3, array[1].AsInt32);
            }
        }

        [Test]
        public void TestPopLast()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);

                var document = new BsonDocument("x", new BsonArray { 1, 2, 3 });
                collection.RemoveAll();
                collection.Insert(document);

                var query = Query.EQ("_id", document["_id"]);
                var update = Update.PopLast("x");
                collection.Update(query, update);

                document = collection.FindOne();
                var array = document["x"].AsBsonArray;
                Assert.AreEqual(2, array.Count);
                Assert.AreEqual(1, array[0].AsInt32);
                Assert.AreEqual(2, array[1].AsInt32);
            }
        }
    }
}
