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
                case BsonType.Array: return (BsonValue)BsonValueSerializers.BsonArraySerializer.Deserialize(bsonReader, typeof(BsonArray), options);
                case BsonType.Binary: return (BsonValue)BsonValueSerializers.BsonBinaryDataSerializer.Deserialize(bsonReader, typeof(BsonBinaryData), options);
                case BsonType.Boolean: return (BsonValue)BsonValueSerializers.BsonBooleanSerializer.Deserialize(bsonReader, typeof(BsonBoolean), options);
                case BsonType.DateTime: return (BsonValue)BsonValueSerializers.BsonDateTimeSerializer.Deserialize(bsonReader, typeof(BsonDateTime), options);
                case BsonType.Document: return (BsonValue)BsonValueSerializers.BsonDocumentSerializer.Deserialize(bsonReader, typeof(BsonDocument), options);
                case BsonType.Double: return (BsonValue)BsonValueSerializers.BsonDoubleSerializer.Deserialize(bsonReader, typeof(BsonDouble), options);
                case BsonType.Int32: return (BsonValue)BsonValueSerializers.BsonInt32Serializer.Deserialize(bsonReader, typeof(BsonInt32), options);
                case BsonType.Int64: return (BsonValue)BsonValueSerializers.BsonInt64Serializer.Deserialize(bsonReader, typeof(BsonInt64), options);
                case BsonType.JavaScript: return (BsonValue)BsonValueSerializers.BsonJavaScriptSerializer.Deserialize(bsonReader, typeof(BsonJavaScript), options);
                case BsonType.JavaScriptWithScope: return (BsonValue)BsonValueSerializers.BsonJavaScriptWithScopeSerializer.Deserialize(bsonReader, typeof(BsonJavaScriptWithScope), options);
                case BsonType.MaxKey: return (BsonValue)BsonValueSerializers.BsonMaxKeySerializer.Deserialize(bsonReader, typeof(BsonMaxKey), options);
                case BsonType.MinKey: return (BsonValue)BsonValueSerializers.BsonMinKeySerializer.Deserialize(bsonReader, typeof(BsonMinKey), options);
                case BsonType.Null: return (BsonValue)BsonValueSerializers.BsonNullSerializer.Deserialize(bsonReader, typeof(BsonNull), options);
                case BsonType.ObjectId: return (BsonValue)BsonValueSerializers.BsonObjectIdSerializer.Deserialize(bsonReader, typeof(BsonObjectId), options);
                case BsonType.RegularExpression: return (BsonValue)BsonValueSerializers.BsonRegularExpressionSerializer.Deserialize(bsonReader, typeof(BsonRegularExpression), options);
                case BsonType.String: return (BsonValue)BsonValueSerializers.BsonStringSerializer.Deserialize(bsonReader, typeof(BsonString), options);
                case BsonType.Symbol: return (BsonValue)BsonValueSerializers.BsonSymbolSerializer.Deserialize(bsonReader, typeof(BsonSymbol), options);
                case BsonType.Timestamp: return (BsonValue)BsonValueSerializers.BsonTimestampSerializer.Deserialize(bsonReader, typeof(BsonTimestamp), options);
                case BsonType.Undefined: return (BsonValue)BsonValueSerializers.BsonUndefinedSerializer.Deserialize(bsonReader, typeof(BsonUndefined), options);
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
                case BsonType.Array: BsonValueSerializers.BsonArraySerializer.Serialize(bsonWriter, typeof(BsonArray), bsonValue, options); break;
                case BsonType.Binary: BsonValueSerializers.BsonBinaryDataSerializer.Serialize(bsonWriter, typeof(BsonBinaryData), bsonValue, options); break;
                case BsonType.Boolean: BsonValueSerializers.BsonBooleanSerializer.Serialize(bsonWriter, typeof(BsonBoolean), bsonValue, options); break;
                case BsonType.DateTime: BsonValueSerializers.BsonDateTimeSerializer.Serialize(bsonWriter, typeof(BsonDateTime), bsonValue, options); break;
                case BsonType.Document: BsonValueSerializers.BsonDocumentSerializer.Serialize(bsonWriter, typeof(BsonDocument), bsonValue, options); break;
                case BsonType.Double: BsonValueSerializers.BsonDoubleSerializer.Serialize(bsonWriter, typeof(BsonDouble), bsonValue, options); break;
                case BsonType.Int32: BsonValueSerializers.BsonInt32Serializer.Serialize(bsonWriter, typeof(BsonInt32), bsonValue, options); break;
                case BsonType.Int64: BsonValueSerializers.BsonInt64Serializer.Serialize(bsonWriter, typeof(BsonInt64), bsonValue, options); break;
                case BsonType.JavaScript: BsonValueSerializers.BsonJavaScriptSerializer.Serialize(bsonWriter, typeof(BsonJavaScript), bsonValue, options); break;
                case BsonType.JavaScriptWithScope: BsonValueSerializers.BsonJavaScriptWithScopeSerializer.Serialize(bsonWriter, typeof(BsonJavaScriptWithScope), bsonValue, options); break;
                case BsonType.MaxKey: BsonValueSerializers.BsonMaxKeySerializer.Serialize(bsonWriter, typeof(BsonMaxKey), bsonValue, options); break;
                case BsonType.MinKey: BsonValueSerializers.BsonMinKeySerializer.Serialize(bsonWriter, typeof(BsonMinKey), bsonValue, options); break;
                case BsonType.Null: BsonValueSerializers.BsonNullSerializer.Serialize(bsonWriter, typeof(BsonNull), bsonValue, options); break;
                case BsonType.ObjectId: BsonValueSerializers.BsonObjectIdSerializer.Serialize(bsonWriter, typeof(BsonObjectId), bsonValue, options); break;
                case BsonType.RegularExpression: BsonValueSerializers.BsonRegularExpressionSerializer.Serialize(bsonWriter, typeof(BsonRegularExpression), bsonValue, options); break;
                case BsonType.String: BsonValueSerializers.BsonStringSerializer.Serialize(bsonWriter, typeof(BsonString), bsonValue, options); break;
                case BsonType.Symbol: BsonValueSerializers.BsonSymbolSerializer.Serialize(bsonWriter, typeof(BsonSymbol), bsonValue, options); break;
                case BsonType.Timestamp: BsonValueSerializers.BsonTimestampSerializer.Serialize(bsonWriter, typeof(BsonTimestamp), bsonValue, options); break;
                case BsonType.Undefined: BsonValueSerializers.BsonUndefinedSerializer.Serialize(bsonWriter, typeof(BsonUndefined), bsonValue, options); break;
                default: throw new BsonInternalException("Invalid BsonType.");
            }
        }
    }
}
