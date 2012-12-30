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
using System.IO;
using System.Linq;
using System.Text;

using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a registry of serializers.
    /// </summary>
    public static class SerializerRegistry
    {
        // private static fields
        private static IBsonSerializer __bsonArraySerializer = new BsonArraySerializer();
        private static IBsonSerializer __bsonBinaryDataSerializer = new BsonBinaryDataSerializer();
        private static IBsonSerializer __bsonBooleanSerializer = new BsonBooleanSerializer();
        private static IBsonSerializer __bsonDateTimeSerializer = new BsonDateTimeSerializer();
        private static IBsonSerializer __bsonDocumentSerializer = new BsonDocumentSerializer();
        private static IBsonSerializer __bsonDocumentWrapperSerializer = new BsonDocumentWrapperSerializer();
        private static IBsonSerializer __bsonDoubleSerializer = new BsonDoubleSerializer();
        private static IBsonSerializer __bsonInt32Serializer = new BsonInt32Serializer();
        private static IBsonSerializer __bsonInt64Serializer = new BsonInt64Serializer();
        private static IBsonSerializer __bsonJavaScriptSerializer = new BsonJavaScriptSerializer();
        private static IBsonSerializer __bsonJavaScriptWithScopeSerializer = new BsonJavaScriptWithScopeSerializer();
        private static IBsonSerializer __bsonMaxKeySerializer = new BsonMaxKeySerializer();
        private static IBsonSerializer __bsonMinKeySerializer = new BsonMinKeySerializer();
        private static IBsonSerializer __bsonNullSerializer = new BsonNullSerializer();
        private static IBsonSerializer __bsonObjectIdSerializer = new BsonObjectIdSerializer();
        private static IBsonSerializer __bsonRegularExpressionSerializer = new BsonRegularExpressionSerializer();
        private static IBsonSerializer __bsonStringSerializer = new BsonStringSerializer();
        private static IBsonSerializer __bsonSymbolSerializer = new BsonSymbolSerializer();
        private static IBsonSerializer __bsonTimestampSerializer = new BsonTimestampSerializer();
        private static IBsonSerializer __bsonUndefinedSerializer = new BsonUndefinedSerializer();
        private static IBsonSerializer __bsonValueSerializer = new BsonValueSerializer();

        // public static properties
        /// <summary>
        /// Gets or sets the serializer for BsonArrays.
        /// </summary>
        public static IBsonSerializer BsonArraySerializer
        {
            get { return __bsonArraySerializer; }
            set { __bsonArraySerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonBinaryDatas.
        /// </summary>
        public static IBsonSerializer BsonBinaryDataSerializer
        {
            get { return __bsonBinaryDataSerializer; }
            set { __bsonBinaryDataSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonBooleans.
        /// </summary>
        public static IBsonSerializer BsonBooleanSerializer
        {
            get { return __bsonBooleanSerializer; }
            set { __bsonBooleanSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonDateTimes.
        /// </summary>
        public static IBsonSerializer BsonDateTimeSerializer
        {
            get { return __bsonDateTimeSerializer; }
            set { __bsonDateTimeSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonDocuments.
        /// </summary>
        public static IBsonSerializer BsonDocumentSerializer
        {
            get { return __bsonDocumentSerializer; }
            set { __bsonDocumentSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonDocumentWrappers.
        /// </summary>
        public static IBsonSerializer BsonDocumentWrapperSerializer
        {
            get { return __bsonDocumentWrapperSerializer; }
            set { __bsonDocumentWrapperSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonDoubles.
        /// </summary>
        public static IBsonSerializer BsonDoubleSerializer
        {
            get { return __bsonDoubleSerializer; }
            set { __bsonDoubleSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonInt32s.
        /// </summary>
        public static IBsonSerializer BsonInt32Serializer
        {
            get { return __bsonInt32Serializer; }
            set { __bsonInt32Serializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonInt64s.
        /// </summary>
        public static IBsonSerializer BsonInt64Serializer
        {
            get { return __bsonInt64Serializer; }
            set { __bsonInt64Serializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonJavaScripts.
        /// </summary>
        public static IBsonSerializer BsonJavaScriptSerializer
        {
            get { return __bsonJavaScriptSerializer; }
            set { __bsonJavaScriptSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonJavaScriptWithScopes.
        /// </summary>
        public static IBsonSerializer BsonJavaScriptWithScopeSerializer
        {
            get { return __bsonJavaScriptWithScopeSerializer; }
            set { __bsonJavaScriptWithScopeSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonMaxKeys.
        /// </summary>
        public static IBsonSerializer BsonMaxKeySerializer
        {
            get { return __bsonMaxKeySerializer; }
            set { __bsonMaxKeySerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonMinKeys.
        /// </summary>
        public static IBsonSerializer BsonMinKeySerializer
        {
            get { return __bsonMinKeySerializer; }
            set { __bsonMinKeySerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonNulls.
        /// </summary>
        public static IBsonSerializer BsonNullSerializer
        {
            get { return __bsonNullSerializer; }
            set { __bsonNullSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonObjectIds.
        /// </summary>
        public static IBsonSerializer BsonObjectIdSerializer
        {
            get { return __bsonObjectIdSerializer; }
            set { __bsonObjectIdSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonRegularExpressions.
        /// </summary>
        public static IBsonSerializer BsonRegularExpressionSerializer
        {
            get { return __bsonRegularExpressionSerializer; }
            set { __bsonRegularExpressionSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonStrings.
        /// </summary>
        public static IBsonSerializer BsonStringSerializer
        {
            get { return __bsonStringSerializer; }
            set { __bsonStringSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonSymbols.
        /// </summary>
        public static IBsonSerializer BsonSymbolSerializer
        {
            get { return __bsonSymbolSerializer; }
            set { __bsonSymbolSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonTimestamps.
        /// </summary>
        public static IBsonSerializer BsonTimestampSerializer
        {
            get { return __bsonTimestampSerializer; }
            set { __bsonTimestampSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonUndefineds.
        /// </summary>
        public static IBsonSerializer BsonUndefinedSerializer
        {
            get { return __bsonUndefinedSerializer; }
            set { __bsonUndefinedSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer for BsonValues.
        /// </summary>
        public static IBsonSerializer BsonValueSerializer
        {
            get { return __bsonValueSerializer; }
            set { __bsonValueSerializer = value; }
        }
    }
}
