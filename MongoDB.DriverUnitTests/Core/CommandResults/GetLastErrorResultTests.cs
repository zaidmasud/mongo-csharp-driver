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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests.CommandResults
{
    [TestFixture]
    public class GetLastErrorResultTests
    {
        [Test]
        public void TestInsert()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);

                collection.Insert(new BsonDocument());
                var result = session.GetLastError();
                Assert.IsFalse(result.HasLastErrorMessage);
                Assert.IsFalse(result.UpdatedExisting);
                Assert.AreEqual(0, result.DocumentsAffected); // note: DocumentsAffected is only set after an Update?
            }
        }

        [Test]
        public void TestUpdate()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);

                var id = ObjectId.GenerateNewId();
                var document = new BsonDocument
                {
                    { "_id", id },
                    { "x", 1 }
                };
                collection.Insert(document);

                var query = Query.EQ("_id", id);
                var update = Update.Inc("x", 1);
                collection.Update(query, update);
                var result = session.GetLastError();
                Assert.IsFalse(result.HasLastErrorMessage);
                Assert.IsTrue(result.UpdatedExisting);
                Assert.AreEqual(1, result.DocumentsAffected);
            }
        }
    }
}
