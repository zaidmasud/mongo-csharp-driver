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
using System.Reflection;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// A deserializer that utilizes a list of BsonSerializationInfos to dump information into a Dictionary&lt;string, object&gt;.
    /// </summary>
    internal class ProjectionDeserializer : BsonBaseSerializer
    {
        // private fields
        private readonly Dictionary<string, BsonSerializationInfo> _deserializationMap;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionDeserializer"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        public ProjectionDeserializer(IEnumerable<BsonSerializationInfo> serializationInfo)
        {
            _deserializationMap = serializationInfo.Distinct(new BsonSerializationInfoElementNameEqualityComparer()).ToDictionary(x => x.ElementName, x => x);
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            var flattenedValues = new Dictionary<string, object>();
            var type = bsonReader.GetCurrentBsonType();
            ReadDocument(bsonReader, flattenedValues, null);
            return flattenedValues;
        }

        // private methods
        private string BuildElementName(string prefix, string name)
        {
            if (prefix == null)
            {
                return name;
            }

            return prefix + "." + name;
        }

        private void ReadDocument(BsonReader bsonReader, Dictionary<string, object> flattenedValues, string prefix)
        {
            bsonReader.ReadStartDocument();
            BsonType bsonType = BsonType.Document;
            while (bsonType != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                var nameWithPrefix = BuildElementName(prefix, name);
                BsonSerializationInfo serializationInfo;
                if (_deserializationMap.TryGetValue(nameWithPrefix, out serializationInfo))
                {
                    var value = serializationInfo.Serializer.Deserialize(bsonReader, serializationInfo.NominalType, serializationInfo.SerializationOptions);
                    flattenedValues.Add(nameWithPrefix, value);
                }
                else
                {
                    if (bsonType == BsonType.Document)
                    {
                        ReadDocument(bsonReader, flattenedValues, nameWithPrefix);
                    }
                }
                bsonType = bsonReader.ReadBsonType();
            }
            bsonReader.ReadEndDocument();
        }

        // nested classes
        private class BsonSerializationInfoElementNameEqualityComparer : IEqualityComparer<BsonSerializationInfo>
        {
            public bool Equals(BsonSerializationInfo x, BsonSerializationInfo y)
            {
                return x.ElementName == y.ElementName;
            }

            public int GetHashCode(BsonSerializationInfo obj)
            {
                return obj.ElementName.GetHashCode();
            }
        }
    }
}
