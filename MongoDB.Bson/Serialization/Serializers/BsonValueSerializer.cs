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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonValues.
    /// </summary>
    public class BsonValueSerializer : BsonBaseSerializer
    {
        // private static fields
        private static BsonValueSerializer __instance = new BsonValueSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonValueSerializer class.
        /// </summary>
        public BsonValueSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonValueSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static BsonValueSerializer Instance
        {
            get { return __instance; }
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
                case BsonType.Array: return (BsonValue)SerializerRegistry.BsonArraySerializer.Deserialize(bsonReader, typeof(BsonArray), options);
                case BsonType.Binary: return (BsonValue)SerializerRegistry.BsonBinaryDataSerializer.Deserialize(bsonReader, typeof(BsonBinaryData), options);
                case BsonType.Boolean: return (BsonValue)SerializerRegistry.BsonBooleanSerializer.Deserialize(bsonReader, typeof(BsonBoolean), options);
                case BsonType.DateTime: return (BsonValue)SerializerRegistry.BsonDateTimeSerializer.Deserialize(bsonReader, typeof(BsonDateTime), options);
                case BsonType.Document: return (BsonValue)SerializerRegistry.BsonDocumentSerializer.Deserialize(bsonReader, typeof(BsonDocument), options);
                case BsonType.Double: return (BsonValue)SerializerRegistry.BsonDoubleSerializer.Deserialize(bsonReader, typeof(BsonDouble), options);
                case BsonType.Int32: return (BsonValue)SerializerRegistry.BsonInt32Serializer.Deserialize(bsonReader, typeof(BsonInt32), options);
                case BsonType.Int64: return (BsonValue)SerializerRegistry.BsonInt64Serializer.Deserialize(bsonReader, typeof(BsonInt64), options);
                case BsonType.JavaScript: return (BsonValue)SerializerRegistry.BsonJavaScriptSerializer.Deserialize(bsonReader, typeof(BsonJavaScript), options);
                case BsonType.JavaScriptWithScope: return (BsonValue)SerializerRegistry.BsonJavaScriptWithScopeSerializer.Deserialize(bsonReader, typeof(BsonJavaScriptWithScope), options);
                case BsonType.MaxKey: return (BsonValue)SerializerRegistry.BsonMaxKeySerializer.Deserialize(bsonReader, typeof(BsonMaxKey), options);
                case BsonType.MinKey: return (BsonValue)SerializerRegistry.BsonMinKeySerializer.Deserialize(bsonReader, typeof(BsonMinKey), options);
                case BsonType.Null: return (BsonValue)SerializerRegistry.BsonNullSerializer.Deserialize(bsonReader, typeof(BsonNull), options);
                case BsonType.ObjectId: return (BsonValue)SerializerRegistry.BsonObjectIdSerializer.Deserialize(bsonReader, typeof(BsonObjectId), options);
                case BsonType.RegularExpression: return (BsonValue)SerializerRegistry.BsonRegularExpressionSerializer.Deserialize(bsonReader, typeof(BsonRegularExpression), options);
                case BsonType.String: return (BsonValue)SerializerRegistry.BsonStringSerializer.Deserialize(bsonReader, typeof(BsonString), options);
                case BsonType.Symbol: return (BsonValue)SerializerRegistry.BsonSymbolSerializer.Deserialize(bsonReader, typeof(BsonSymbol), options);
                case BsonType.Timestamp: return (BsonValue)SerializerRegistry.BsonTimestampSerializer.Deserialize(bsonReader, typeof(BsonTimestamp), options);
                case BsonType.Undefined: return (BsonValue)SerializerRegistry.BsonUndefinedSerializer.Deserialize(bsonReader, typeof(BsonUndefined), options);
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
                case BsonType.Array: SerializerRegistry.BsonArraySerializer.Serialize(bsonWriter, typeof(BsonArray), bsonValue, options); break;
                case BsonType.Binary: SerializerRegistry.BsonBinaryDataSerializer.Serialize(bsonWriter, typeof(BsonBinaryData), bsonValue, options); break;
                case BsonType.Boolean: SerializerRegistry.BsonBooleanSerializer.Serialize(bsonWriter, typeof(BsonBoolean), bsonValue, options); break;
                case BsonType.DateTime: SerializerRegistry.BsonDateTimeSerializer.Serialize(bsonWriter, typeof(BsonDateTime), bsonValue, options); break;
                case BsonType.Document: SerializerRegistry.BsonDocumentSerializer.Serialize(bsonWriter, typeof(BsonDocument), bsonValue, options); break;
                case BsonType.Double: SerializerRegistry.BsonDoubleSerializer.Serialize(bsonWriter, typeof(BsonDouble), bsonValue, options); break;
                case BsonType.Int32: SerializerRegistry.BsonInt32Serializer.Serialize(bsonWriter, typeof(BsonInt32), bsonValue, options); break;
                case BsonType.Int64: SerializerRegistry.BsonInt64Serializer.Serialize(bsonWriter, typeof(BsonInt64), bsonValue, options); break;
                case BsonType.JavaScript: SerializerRegistry.BsonJavaScriptSerializer.Serialize(bsonWriter, typeof(BsonJavaScript), bsonValue, options); break;
                case BsonType.JavaScriptWithScope: SerializerRegistry.BsonJavaScriptWithScopeSerializer.Serialize(bsonWriter, typeof(BsonJavaScriptWithScope), bsonValue, options); break;
                case BsonType.MaxKey: SerializerRegistry.BsonMaxKeySerializer.Serialize(bsonWriter, typeof(BsonMaxKey), bsonValue, options); break;
                case BsonType.MinKey: SerializerRegistry.BsonMinKeySerializer.Serialize(bsonWriter, typeof(BsonMinKey), bsonValue, options); break;
                case BsonType.Null: SerializerRegistry.BsonNullSerializer.Serialize(bsonWriter, typeof(BsonNull), bsonValue, options); break;
                case BsonType.ObjectId: SerializerRegistry.BsonObjectIdSerializer.Serialize(bsonWriter, typeof(BsonObjectId), bsonValue, options); break;
                case BsonType.RegularExpression: SerializerRegistry.BsonRegularExpressionSerializer.Serialize(bsonWriter, typeof(BsonRegularExpression), bsonValue, options); break;
                case BsonType.String: SerializerRegistry.BsonStringSerializer.Serialize(bsonWriter, typeof(BsonString), bsonValue, options); break;
                case BsonType.Symbol: SerializerRegistry.BsonSymbolSerializer.Serialize(bsonWriter, typeof(BsonSymbol), bsonValue, options); break;
                case BsonType.Timestamp: SerializerRegistry.BsonTimestampSerializer.Serialize(bsonWriter, typeof(BsonTimestamp), bsonValue, options); break;
                case BsonType.Undefined: SerializerRegistry.BsonUndefinedSerializer.Serialize(bsonWriter, typeof(BsonUndefined), bsonValue, options); break;
                default: throw new BsonInternalException("Invalid BsonType.");
            }
        }
    }
}
