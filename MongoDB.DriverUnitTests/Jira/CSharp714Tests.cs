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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.UnitTests;
using NUnit.Framework;

namespace MongoDB.DriverUnitTests.Jira.CSharp714
{
    [TestFixture]
    public class CSharp714Tests
    {
        
        public class C
        {
            public ObjectId Id { get; set; }
            public int I { get; set; }
            public Guid Guid { get; set; }
        }

        private MongoServer _server;
        private MongoDatabase _database;
        private MongoCollection<C> _collection;
        private IIdGenerator _generator = new AscendingGuidGenerator();
        
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            _server = Configuration.TestServer;
            _database = Configuration.TestDatabase;
            var collSettings = new MongoCollectionSettings();
            collSettings.GuidRepresentation = GuidRepresentation.Standard;
            _collection = _database.GetCollection<C> (
                "csharp714",
                collSettings);
            _collection.Drop();
        }
        
        [Test]
        public void TestGuid()
        {
            _collection.RemoveAll();
            CreateTestData();
            var cursor = _collection.FindAll().SetSortOrder("Guid");
            var initId = 0;
            foreach (var c in cursor) 
            {
                Assert.AreEqual(initId++, c.I);
            }
        }

        private void CreateTestData()
        {
            for (var i=0; i<10; i++) 
            {
                _collection.Insert(
                    new C()
                    {
                        I = i,
                        Guid = (Guid) _generator.GenerateId(null, null)
                    }
                );
            }
            _collection.CreateIndex("Guid");
        }
    }
}
