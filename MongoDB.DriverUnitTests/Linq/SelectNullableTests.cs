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
using MongoDB.Driver.Linq;

namespace MongoDB.DriverUnitTests.Linq
{
    [TestFixture]
    public class SelectNullableTests
    {
        private enum E { None, A, B };

        private class C
        {
            public ObjectId Id { get; set; }
            [BsonElement("e")]
            [BsonRepresentation(BsonType.String)]
            public E? E { get; set; }
            [BsonElement("x")]
            public int? X { get; set; }
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                collection.Drop();
                collection.Insert(new C { E = null });
                collection.Insert(new C { E = E.A });
                collection.Insert(new C { E = E.B });
                collection.Insert(new C { X = null });
                collection.Insert(new C { X = 1 });
                collection.Insert(new C { X = 2 });
            }
        }

        [Test]
        public void TestWhereEEqualsA()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = from c in collection.AsQueryable<C>()
                            where c.E == E.A
                            select c;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(C), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(C c) => ((Nullable<Int32>)c.E == (Nullable<Int32>)1)", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"e\" : \"A\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(1, Consume(query));
            }
        }

        [Test]
        public void TestWhereEEqualsNull()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = from c in collection.AsQueryable<C>()
                            where c.E == null
                            select c;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(C), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(C c) => ((Nullable<Int32>)c.E == (Nullable<Int32>)null)", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"e\" : null }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(4, Consume(query));
            }
        }

        [Test]
        public void TestWhereXEquals1()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = from c in collection.AsQueryable<C>()
                            where c.X == 1
                            select c;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(C), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(C c) => (c.X == (Nullable<Int32>)1)", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"x\" : 1 }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(1, Consume(query));
            }
        }

        [Test]
        public void TestWhereXEqualsNull()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = from c in collection.AsQueryable<C>()
                            where c.X == null
                            select c;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(C), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(C c) => (c.X == (Nullable<Int32>)null)", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"x\" : null }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(4, Consume(query));
            }
        }

        private int Consume<T>(IQueryable<T> query)
        {
            var count = 0;
            foreach (var c in query)
            {
                count++;
            }
            return count;
        }
    }
}
