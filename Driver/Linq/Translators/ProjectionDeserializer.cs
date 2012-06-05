using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Reflection;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// A deserializer that utilizes a list of BsonSerializationInfos to dump information into a Dictionary<string, object>.
    /// </summary>
    internal class ProjectionDeserializer : BsonBaseSerializer
    {
        private readonly Dictionary<string, BsonSerializationInfo> _deserializationMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionDeserializer"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        public ProjectionDeserializer(IEnumerable<BsonSerializationInfo> serializationInfo)
        {
            _deserializationMap = serializationInfo.Distinct(new BsonSerialiationInfoElementNameEqualityComparer()).ToDictionary(x => x.ElementName, x => x);
        }

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
            var flattendValues = new Dictionary<string, object>();
            var type = bsonReader.GetCurrentBsonType();
            ReadDocument(null, bsonReader, flattendValues);
            return flattendValues;
        }

        private void ReadDocument(string prefix, BsonReader bsonReader, Dictionary<string, object> flattenedValues)
        {
            bsonReader.ReadStartDocument();
            BsonType type = BsonType.Document;
            while (type != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                var nameWithPrefix = BuildElementName(prefix, name);
                BsonSerializationInfo info;
                if (_deserializationMap.TryGetValue(nameWithPrefix, out info))
                {
                    var value = info.Serializer.Deserialize(bsonReader, info.NominalType, info.SerializationOptions);
                    flattenedValues.Add(nameWithPrefix, value);
                }
                else
                {
                    if (type == BsonType.Document)
                    {
                        ReadDocument(nameWithPrefix, bsonReader, flattenedValues);
                    }
                }
                type = bsonReader.ReadBsonType();
            }
            bsonReader.ReadEndDocument();
        }

        private static string BuildElementName(string prefix, string name)
        {
            if (prefix == null)
            {
                return name;
            }

            return prefix + "." + name;
        }

        private class BsonSerialiationInfoElementNameEqualityComparer : IEqualityComparer<BsonSerializationInfo>
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