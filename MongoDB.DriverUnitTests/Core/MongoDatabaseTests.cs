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

namespace MongoDB.DriverUnitTests
{
    [TestFixture]
    public class MongoDatabaseTests
    {
        // TODO: more tests for MongoDatabase

        [Test]
        public void TestCollectionExists()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collectionName = "testcollectionexists";
                Assert.IsFalse(database.CollectionExists(collectionName));

                database.GetCollection(collectionName).Insert(new BsonDocument());
                Assert.IsTrue(database.CollectionExists(collectionName));
            }
        }

        [Test]
        public void TestConstructorArgumentChecking()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var settings = new MongoDatabaseSettings();
                Assert.Throws<ArgumentNullException>(() => { new MongoDatabase(null, "name", settings); });
                Assert.Throws<ArgumentNullException>(() => { new MongoDatabase(session, null, settings); });
                Assert.Throws<ArgumentNullException>(() => { new MongoDatabase(session, "name", null); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { new MongoDatabase(session, "", settings); });
            }
        }

        [Test]
        public void TestCreateCollection()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collectionName = "testcreatecollection";
                Assert.IsFalse(database.CollectionExists(collectionName));

                database.CreateCollection(collectionName);
                Assert.IsTrue(database.CollectionExists(collectionName));
            }
        }

        [Test]
        public void TestDropCollection()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collectionName = "testdropcollection";
                Assert.IsFalse(database.CollectionExists(collectionName));

                database.GetCollection(collectionName).Insert(new BsonDocument());
                Assert.IsTrue(database.CollectionExists(collectionName));

                database.DropCollection(collectionName);
                Assert.IsFalse(database.CollectionExists(collectionName));
            }
        }

        [Test]
        public void TestEvalNoArgs()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var code = "function() { return 1; }";
                var result = database.Eval(code);
                Assert.AreEqual(1, result.ToInt32());
            }
        }

        [Test]
        public void TestEvalNoArgsNoLock()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var code = "function() { return 1; }";
                var result = database.Eval(EvalFlags.NoLock, code);
                Assert.AreEqual(1, result.ToInt32());
            }
        }

        [Test]
        public void TestEvalWithArgs()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var code = "function(x, y) { return x / y; }";
                var result = database.Eval(code, 6, 2);
                Assert.AreEqual(3, result.ToInt32());
            }
        }

        [Test]
        public void TestEvalWithArgsNoLock()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var code = "function(x, y) { return x / y; }";
                var result = database.Eval(EvalFlags.NoLock, code, 6, 2);
                Assert.AreEqual(3, result.ToInt32());
            }
        }

        [Test]
        public void TestFetchDBRef()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collectionName = "testdbref";
                var collection = database.GetCollection(collectionName);
                var document = new BsonDocument { { "_id", ObjectId.GenerateNewId() }, { "P", "x" } };
                collection.Insert(document);

                var dbRef = new MongoDBRef(collectionName, document["_id"].AsObjectId);
                var fetched = database.FetchDBRef(dbRef);
                Assert.AreEqual(document, fetched);
                Assert.AreEqual(document.ToJson(), fetched.ToJson());

                var dbRefWithDatabaseName = new MongoDBRef(database.Name, collectionName, document["_id"].AsObjectId);
                fetched = session.FetchDBRef(dbRefWithDatabaseName);
                Assert.AreEqual(document, fetched);
                Assert.AreEqual(document.ToJson(), fetched.ToJson());
                Assert.Throws<ArgumentException>(() => { session.FetchDBRef(dbRef); });
            }
        }

        [Test]
        public void TestGetCollection()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collectionName = Configuration.TestCollectionName;
                var collection = database.GetCollection(typeof(BsonDocument), collectionName);
                Assert.AreSame(database, collection.Database);
                Assert.AreEqual(database.Name + "." + collectionName, collection.FullName);
                Assert.AreEqual(collectionName, collection.Name);
                Assert.AreEqual(database.Settings.SafeMode, collection.Settings.SafeMode);
            }
        }

        [Test]
        public void TestGetCollectionGeneric()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collectionName = Configuration.TestCollectionName;
                var collection = database.GetCollection(collectionName);
                Assert.AreSame(database, collection.Database);
                Assert.AreEqual(database.Name + "." + collectionName, collection.FullName);
                Assert.AreEqual(collectionName, collection.Name);
                Assert.AreEqual(database.Settings.SafeMode, collection.Settings.SafeMode);
            }
        }

        [Test]
        public void TestGetCollectionNames()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                database.Drop();
                database.GetCollection("a").Insert(new BsonDocument("a", 1));
                database.GetCollection("b").Insert(new BsonDocument("b", 1));
                database.GetCollection("c").Insert(new BsonDocument("c", 1));
                var collectionNames = database.GetCollectionNames();
                Assert.AreEqual(new[] { "a", "b", "c", "system.indexes" }, collectionNames);
            }
        }

        [Test]
        public void TestGetProfilingInfo()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var instance = session.ServerInstance;
                if (instance.InstanceType != MongoServerInstanceType.ShardRouter)
                {
                    var collection = database.GetCollection(Configuration.TestCollectionName);
                    if (collection.Exists()) { collection.Drop(); }
                    collection.Insert(new BsonDocument("x", 1));
                    database.SetProfilingLevel(ProfilingLevel.All);
                    var count = collection.Count();
                    database.SetProfilingLevel(ProfilingLevel.None);
                    var info = database.GetProfilingInfo(Query.Null).SetSortOrder(SortBy.Descending("$natural")).SetLimit(1).First();
                    Assert.IsTrue(info.Timestamp >= new DateTime(2011, 10, 6, 0, 0, 0, DateTimeKind.Utc));
                    Assert.IsTrue(info.Duration >= TimeSpan.Zero);
                }
            }
        }

        [Test]
        public void TestIsCollectionNameValid()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                string message;
                Assert.Throws<ArgumentNullException>(() => { database.IsCollectionNameValid(null, out message); });
                Assert.IsFalse(database.IsCollectionNameValid("", out message));
                Assert.IsFalse(database.IsCollectionNameValid("a\0b", out message));
                Assert.IsFalse(database.IsCollectionNameValid(new string('x', 128), out message));
            }
        }

        [Test]
        public void TestRenameCollection()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collectionName1 = "testrenamecollection1";
                var collectionName2 = "testrenamecollection2";
                Assert.IsFalse(database.CollectionExists(collectionName1));
                Assert.IsFalse(database.CollectionExists(collectionName2));

                database.GetCollection(collectionName1).Insert(new BsonDocument());
                Assert.IsTrue(database.CollectionExists(collectionName1));
                Assert.IsFalse(database.CollectionExists(collectionName2));

                database.RenameCollection(collectionName1, collectionName2);
                Assert.IsFalse(database.CollectionExists(collectionName1));
                Assert.IsTrue(database.CollectionExists(collectionName2));
            }
        }

        [Test]
        public void TestRenameCollectionArgumentChecking()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                Assert.Throws<ArgumentNullException>(() => { database.RenameCollection(null, "new"); });
                Assert.Throws<ArgumentNullException>(() => { database.RenameCollection("old", null); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { database.RenameCollection("old", ""); });
            }
        }

        [Test]
        public void TestRenameCollectionDropTarget()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                const string collectionName1 = "testrenamecollectiondroptarget1";
                const string collectionName2 = "testrenamecollectiondroptarget2";
                Assert.IsFalse(database.CollectionExists(collectionName1));
                Assert.IsFalse(database.CollectionExists(collectionName2));

                database.GetCollection(collectionName1).Insert(new BsonDocument());
                database.GetCollection(collectionName2).Insert(new BsonDocument());
                Assert.IsTrue(database.CollectionExists(collectionName1));
                Assert.IsTrue(database.CollectionExists(collectionName2));

                Assert.Throws<MongoCommandException>(() => database.RenameCollection(collectionName1, collectionName2));
                database.RenameCollection(collectionName1, collectionName2, true);
                Assert.IsFalse(database.CollectionExists(collectionName1));
                Assert.IsTrue(database.CollectionExists(collectionName2));
            }
        }

        [Test]
        public void TestSetProfilingLevel()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var instance = session.ServerInstance;
                if (instance.InstanceType != MongoServerInstanceType.ShardRouter)
                {
                    database.SetProfilingLevel(ProfilingLevel.None, TimeSpan.FromMilliseconds(100));
                    var result = database.GetProfilingLevel();
                    Assert.AreEqual(ProfilingLevel.None, result.Level);
                    Assert.AreEqual(TimeSpan.FromMilliseconds(100), result.Slow);

                    database.SetProfilingLevel(ProfilingLevel.Slow);
                    result = database.GetProfilingLevel();
                    Assert.AreEqual(ProfilingLevel.Slow, result.Level);
                    Assert.AreEqual(TimeSpan.FromMilliseconds(100), result.Slow);

                    database.SetProfilingLevel(ProfilingLevel.Slow, TimeSpan.FromMilliseconds(200));
                    result = database.GetProfilingLevel();
                    Assert.AreEqual(ProfilingLevel.Slow, result.Level);
                    Assert.AreEqual(TimeSpan.FromMilliseconds(200), result.Slow);

                    database.SetProfilingLevel(ProfilingLevel.Slow, TimeSpan.FromMilliseconds(100));
                    result = database.GetProfilingLevel();
                    Assert.AreEqual(ProfilingLevel.Slow, result.Level);
                    Assert.AreEqual(TimeSpan.FromMilliseconds(100), result.Slow);

                    database.SetProfilingLevel(ProfilingLevel.All);
                    result = database.GetProfilingLevel();
                    Assert.AreEqual(ProfilingLevel.All, result.Level);
                    Assert.AreEqual(TimeSpan.FromMilliseconds(100), result.Slow);

                    database.SetProfilingLevel(ProfilingLevel.None);
                    result = database.GetProfilingLevel();
                    Assert.AreEqual(ProfilingLevel.None, result.Level);
                    Assert.AreEqual(TimeSpan.FromMilliseconds(100), result.Slow);
                }
            }
        }

        [Test]
        public void TestUserMethods()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection("system.users");
                collection.RemoveAll();
                database.AddUser(new MongoCredentials("username", "password"), true);
                Assert.AreEqual(1, collection.Count());

                var user = database.FindUser("username");
                Assert.AreEqual("username", user.Username);
                Assert.AreEqual(MongoUtils.Hash("username:mongo:password"), user.PasswordHash);
                Assert.AreEqual(true, user.IsReadOnly);

                var users = database.FindAllUsers();
                Assert.AreEqual(1, users.Length);
                Assert.AreEqual("username", users[0].Username);
                Assert.AreEqual(MongoUtils.Hash("username:mongo:password"), users[0].PasswordHash);
                Assert.AreEqual(true, users[0].IsReadOnly);

                database.RemoveUser(user);
                Assert.AreEqual(0, collection.Count());
            }
        }
    }
}
