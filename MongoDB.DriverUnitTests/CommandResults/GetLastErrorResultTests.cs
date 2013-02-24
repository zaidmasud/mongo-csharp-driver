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

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;

namespace MongoDB.DriverUnitTests.CommandResults
{
    [TestFixture]
    public class GetLastErrorResultTests
    {
        private MongoClient _client;
        private MongoServer _server;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            var settings = Configuration.TestClient.Settings.Clone();
            settings.WriteConcern = WriteConcern.Unacknowledged;
            _client = new MongoClient(settings);
            _server = _client.GetServer();
        }

        [Test]
        public void TestInsert()
        {
            using (var connection = _server.GetConnection())
            {
                var database = connection.GetDatabase(Configuration.TestDatabase.Name);
                var collection = database.GetCollection<BsonDocument>(Configuration.TestCollection.Name);

                collection.Insert(new BsonDocument());
                var result = connection.GetLastError();
                Assert.IsFalse(result.HasLastErrorMessage);
                Assert.IsFalse(result.UpdatedExisting);
                Assert.AreEqual(0, result.DocumentsAffected); // note: DocumentsAffected is only set after an Update?
            }
        }

        [Test]
        public void TestUpdate()
        {
            using (var connection = _server.GetConnection())
            {
                var database = connection.GetDatabase(Configuration.TestDatabase.Name);
                var collection = database.GetCollection<BsonDocument>(Configuration.TestCollection.Name);

                var id = ObjectId.GenerateNewId();
                var document = new BsonDocument
                {
                    { "_id", id },
                    { "x", 1 }
                };
                collection.Insert(document);
                Assert.AreEqual(true, connection.GetLastError().Ok);

                var query = Query.EQ("_id", id);
                var update = Update.Inc("x", 1);
                collection.Update(query, update);
                var result = connection.GetLastError();
                Assert.IsFalse(result.HasLastErrorMessage);
                Assert.IsTrue(result.UpdatedExisting);
                Assert.AreEqual(1, result.DocumentsAffected);
            }
        }
    }
}
