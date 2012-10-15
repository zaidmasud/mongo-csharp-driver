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

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonValues.
    /// </summary>
    public class BsonValueSerializer : BsonBaseSerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonValueSerializer class.
        /// </summary>
        public BsonValueSerializer(SerializationContext serializationContext)
            : base(serializationContext)
        {
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
        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, // ignored
            IBsonSerializationOptions options)
        {
            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Array: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonArray)).Deserialize(bsonReader, typeof(BsonArray), options);
                case BsonType.Binary: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonBinaryData)).Deserialize(bsonReader, typeof(BsonBinaryData), options);
                case BsonType.Boolean: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonBoolean)).Deserialize(bsonReader, typeof(BsonBoolean), options);
                case BsonType.DateTime: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonDateTime)).Deserialize(bsonReader, typeof(BsonDateTime), options);
                case BsonType.Document: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonDocument)).Deserialize(bsonReader, typeof(BsonDocument), options);
                case BsonType.Double: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonDouble)).Deserialize(bsonReader, typeof(BsonDouble), options);
                case BsonType.Int32: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonInt32)).Deserialize(bsonReader, typeof(BsonInt32), options);
                case BsonType.Int64: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonInt64)).Deserialize(bsonReader, typeof(BsonInt64), options);
                case BsonType.JavaScript: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonJavaScript)).Deserialize(bsonReader, typeof(BsonJavaScript), options);
                case BsonType.JavaScriptWithScope: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonJavaScriptWithScope)).Deserialize(bsonReader, typeof(BsonJavaScriptWithScope), options);
                case BsonType.MaxKey: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonMaxKey)).Deserialize(bsonReader, typeof(BsonMaxKey), options);
                case BsonType.MinKey: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonMinKey)).Deserialize(bsonReader, typeof(BsonMinKey), options);
                case BsonType.Null: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonNull)).Deserialize(bsonReader, typeof(BsonNull), options);
                case BsonType.ObjectId: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonObjectId)).Deserialize(bsonReader, typeof(BsonObjectId), options);
                case BsonType.RegularExpression: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonRegularExpression)).Deserialize(bsonReader, typeof(BsonRegularExpression), options);
                case BsonType.String: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonString)).Deserialize(bsonReader, typeof(BsonString), options);
                case BsonType.Symbol: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonSymbol)).Deserialize(bsonReader, typeof(BsonSymbol), options);
                case BsonType.Timestamp: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonTimestamp)).Deserialize(bsonReader, typeof(BsonTimestamp), options);
                case BsonType.Undefined: return (BsonValue)SerializationContext.LookupSerializer(typeof(BsonUndefined)).Deserialize(bsonReader, typeof(BsonUndefined), options);
                default:
                    var message = string.Format("Invalid BsonType {0}.", bsonType);
                    throw new BsonInternalException(message);
            }
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var bsonValue = (BsonValue)value;
            switch (bsonValue.BsonType)
            {
                case BsonType.Array: SerializationContext.LookupSerializer(typeof(BsonArray)).Serialize(bsonWriter, typeof(BsonArray), bsonValue, options); break;
                case BsonType.Binary: SerializationContext.LookupSerializer(typeof(BsonBinaryData)).Serialize(bsonWriter, typeof(BsonBinaryData), bsonValue, options); break;
                case BsonType.Boolean: SerializationContext.LookupSerializer(typeof(BsonBoolean)).Serialize(bsonWriter, typeof(BsonBoolean), bsonValue, options); break;
                case BsonType.DateTime: SerializationContext.LookupSerializer(typeof(BsonDateTime)).Serialize(bsonWriter, typeof(BsonDateTime), bsonValue, options); break;
                case BsonType.Document: SerializationContext.LookupSerializer(typeof(BsonDocument)).Serialize(bsonWriter, typeof(BsonDocument), bsonValue, options); break;
                case BsonType.Double: SerializationContext.LookupSerializer(typeof(BsonDouble)).Serialize(bsonWriter, typeof(BsonDouble), bsonValue, options); break;
                case BsonType.Int32: SerializationContext.LookupSerializer(typeof(BsonInt32)).Serialize(bsonWriter, typeof(BsonInt32), bsonValue, options); break;
                case BsonType.Int64: SerializationContext.LookupSerializer(typeof(BsonInt64)).Serialize(bsonWriter, typeof(BsonInt64), bsonValue, options); break;
                case BsonType.JavaScript: SerializationContext.LookupSerializer(typeof(BsonJavaScript)).Serialize(bsonWriter, typeof(BsonJavaScript), bsonValue, options); break;
                case BsonType.JavaScriptWithScope: SerializationContext.LookupSerializer(typeof(BsonJavaScriptWithScope)).Serialize(bsonWriter, typeof(BsonJavaScriptWithScope), bsonValue, options); break;
                case BsonType.MaxKey: SerializationContext.LookupSerializer(typeof(BsonMaxKey)).Serialize(bsonWriter, typeof(BsonMaxKey), bsonValue, options); break;
                case BsonType.MinKey: SerializationContext.LookupSerializer(typeof(BsonMinKey)).Serialize(bsonWriter, typeof(BsonMinKey), bsonValue, options); break;
                case BsonType.Null: SerializationContext.LookupSerializer(typeof(BsonNull)).Serialize(bsonWriter, typeof(BsonNull), bsonValue, options); break;
                case BsonType.ObjectId: SerializationContext.LookupSerializer(typeof(BsonObjectId)).Serialize(bsonWriter, typeof(BsonObjectId), bsonValue, options); break;
                case BsonType.RegularExpression: SerializationContext.LookupSerializer(typeof(BsonRegularExpression)).Serialize(bsonWriter, typeof(BsonRegularExpression), bsonValue, options); break;
                case BsonType.String: SerializationContext.LookupSerializer(typeof(BsonString)).Serialize(bsonWriter, typeof(BsonString), bsonValue, options); break;
                case BsonType.Symbol: SerializationContext.LookupSerializer(typeof(BsonSymbol)).Serialize(bsonWriter, typeof(BsonSymbol), bsonValue, options); break;
                case BsonType.Timestamp: SerializationContext.LookupSerializer(typeof(BsonTimestamp)).Serialize(bsonWriter, typeof(BsonTimestamp), bsonValue, options); break;
                case BsonType.Undefined: SerializationContext.LookupSerializer(typeof(BsonUndefined)).Serialize(bsonWriter, typeof(BsonUndefined), bsonValue, options); break;
                default: throw new BsonInternalException("Invalid BsonType.");
            }
        }
    }
}
