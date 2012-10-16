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
using System.Xml;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.BsonUnitTests.Serialization.Serializers
{
    [TestFixture]
    public class BsonDocumentSerializerTests
    {
        [Test]
        public void TestDuplicateNamesAtTopLevelAreAllowed()
        {
            var json = "{ x : 1, x : 1 }";
            using (var reader = BsonReader.Create(json))
            {
                var serializer = new BsonDocumentSerializer(SerializationConfig.Default);
                var options = new DocumentSerializationOptions { AllowDuplicateNames = true };
                var doc = (BsonDocument)serializer.Deserialize(reader, typeof(BsonDocument), options);
                Assert.AreEqual(2, doc.ElementCount);
                Assert.AreEqual("x", doc.GetElement(0).Name);
                Assert.AreEqual("x", doc.GetElement(1).Name);
            }
        }

        [Test]
        public void TestDuplicateNamesAtTopLevelAreNotAllowed()
        {
            var json = "{ x : 1, x : 1 }";
            Assert.Throws<InvalidOperationException>(() => { var doc = BsonDocument.Parse(json); });
        }

        [Test]
        public void TestDuplicateNamesInNestedArrayAreAllowed()
        {
            var json = "{ a : [{ x : 1, x : 1 }] }";
            using (var reader = BsonReader.Create(json))
            {
                var serializer = new BsonDocumentSerializer(SerializationConfig.Default);
                var options = new DocumentSerializationOptions { AllowDuplicateNames = true };
                var doc = (BsonDocument)serializer.Deserialize(reader, typeof(BsonDocument), options);
                var nestedArray = doc["a"].AsBsonArray;
                var nestedDoc = nestedArray[0].AsBsonDocument;
                Assert.AreEqual(2, nestedDoc.ElementCount);
                Assert.AreEqual("x", nestedDoc.GetElement(0).Name);
                Assert.AreEqual("x", nestedDoc.GetElement(1).Name);
            }
        }

        [Test]
        public void TestDuplicateNamesInNestedArrayAreNotAllowed()
        {
            var json = "{ a : [{ x : 1, x : 1 }] }";
            Assert.Throws<InvalidOperationException>(() => { var doc = BsonDocument.Parse(json); });
        }

        [Test]
        public void TestDuplicateNamesInNestedDocumentAreAllowed()
        {
            var json = "{ n : { x : 1, x : 1 } }";
            using (var reader = BsonReader.Create(json))
            {
                var serializer = new BsonDocumentSerializer(SerializationConfig.Default);
                var options = new DocumentSerializationOptions { AllowDuplicateNames = true };
                var doc = (BsonDocument)serializer.Deserialize(reader, typeof(BsonDocument), options);
                var nestedDoc = doc["n"].AsBsonDocument;
                Assert.AreEqual(2, nestedDoc.ElementCount);
                Assert.AreEqual("x", nestedDoc.GetElement(0).Name);
                Assert.AreEqual("x", nestedDoc.GetElement(1).Name);
            }
        }

        [Test]
        public void TestDuplicateNamesInNestedDocumentAreNotAllowed()
        {
            var json = "{ n : { x : 1, x : 1 } }";
            Assert.Throws<InvalidOperationException>(() => { var doc = BsonDocument.Parse(json); });
        }
    }
}
