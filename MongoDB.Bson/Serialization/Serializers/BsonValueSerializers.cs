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
    /// Represents a cached set of serializers for BsonValue subclasses.
    /// </summary>
    public static class BsonValueSerializers
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
        /// Gets an instance of the BsonArraySerializer class.
        /// </summary>
        public static IBsonSerializer BsonArraySerializer
        {
            get { return __bsonArraySerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonBinaryDataSerializer class.
        /// </summary>
        public static IBsonSerializer BsonBinaryDataSerializer
        {
            get { return __bsonBinaryDataSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonBooleanSerializer class.
        /// </summary>
        public static IBsonSerializer BsonBooleanSerializer
        {
            get { return __bsonBooleanSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonDateTimeSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDateTimeSerializer
        {
            get { return __bsonDateTimeSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonDocumentSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDocumentSerializer
        {
            get { return __bsonDocumentSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonDocumentWrapperSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDocumentWrapperSerializer
        {
            get { return __bsonDocumentWrapperSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonDoubleSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDoubleSerializer
        {
            get { return __bsonDoubleSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonInt32Serializer class.
        /// </summary>
        public static IBsonSerializer BsonInt32Serializer
        {
            get { return __bsonInt32Serializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonInt64Serializer class.
        /// </summary>
        public static IBsonSerializer BsonInt64Serializer
        {
            get { return __bsonInt64Serializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonJavaScriptSerializer class.
        /// </summary>
        public static IBsonSerializer BsonJavaScriptSerializer
        {
            get { return __bsonJavaScriptSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonJavaScriptWithScopeSerializer class.
        /// </summary>
        public static IBsonSerializer BsonJavaScriptWithScopeSerializer
        {
            get { return __bsonJavaScriptWithScopeSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonMaxKeySerializer class.
        /// </summary>
        public static IBsonSerializer BsonMaxKeySerializer
        {
            get { return __bsonMaxKeySerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonMinKeySerializer class.
        /// </summary>
        public static IBsonSerializer BsonMinKeySerializer
        {
            get { return __bsonMinKeySerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonNullSerializer class.
        /// </summary>
        public static IBsonSerializer BsonNullSerializer
        {
            get { return __bsonNullSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonObjectIdSerializer class.
        /// </summary>
        public static IBsonSerializer BsonObjectIdSerializer
        {
            get { return __bsonObjectIdSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonRegularExpressionSerializer class.
        /// </summary>
        public static IBsonSerializer BsonRegularExpressionSerializer
        {
            get { return __bsonRegularExpressionSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonStringSerializer class.
        /// </summary>
        public static IBsonSerializer BsonStringSerializer
        {
            get { return __bsonStringSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonSymbolSerializer class.
        /// </summary>
        public static IBsonSerializer BsonSymbolSerializer
        {
            get { return __bsonSymbolSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonTimestampSerializer class.
        /// </summary>
        public static IBsonSerializer BsonTimestampSerializer
        {
            get { return __bsonTimestampSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonUndefinedSerializer class.
        /// </summary>
        public static IBsonSerializer BsonUndefinedSerializer
        {
            get { return __bsonUndefinedSerializer; }
        }

        /// <summary>
        /// Gets an instance of the BsonValueSerializer class.
        /// </summary>
        public static IBsonSerializer BsonValueSerializer
        {
            get { return __bsonValueSerializer; }
        }
    }
}
