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
        private static readonly object __staticLock = new object();
        private static volatile IBsonSerializer __bsonArraySerializer;
        private static volatile IBsonSerializer __bsonBinaryDataSerializer;
        private static volatile IBsonSerializer __bsonBooleanSerializer;
        private static volatile IBsonSerializer __bsonDateTimeSerializer;
        private static volatile IBsonSerializer __bsonDocumentSerializer;
        private static volatile IBsonSerializer __bsonDocumentWrapperSerializer;
        private static volatile IBsonSerializer __bsonDoubleSerializer;
        private static volatile IBsonSerializer __bsonInt32Serializer;
        private static volatile IBsonSerializer __bsonInt64Serializer;
        private static volatile IBsonSerializer __bsonJavaScriptSerializer;
        private static volatile IBsonSerializer __bsonJavaScriptWithScopeSerializer;
        private static volatile IBsonSerializer __bsonMaxKeySerializer;
        private static volatile IBsonSerializer __bsonMinKeySerializer;
        private static volatile IBsonSerializer __bsonNullSerializer;
        private static volatile IBsonSerializer __bsonObjectIdSerializer;
        private static volatile IBsonSerializer __bsonRegularExpressionSerializer;
        private static volatile IBsonSerializer __bsonStringSerializer;
        private static volatile IBsonSerializer __bsonSymbolSerializer;
        private static volatile IBsonSerializer __bsonTimestampSerializer;
        private static volatile IBsonSerializer __bsonUndefinedSerializer;
        private static volatile IBsonSerializer __bsonValueSerializer;

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonArraySerializer class.
        /// </summary>
        public static IBsonSerializer BsonArraySerializer
        {
            get
            {
                var serializer = __bsonArraySerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonArraySerializer = BsonSerializer.LookupSerializer(typeof(BsonArray));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonBinaryDataSerializer class.
        /// </summary>
        public static IBsonSerializer BsonBinaryDataSerializer
        {
            get
            {
                var serializer = __bsonBinaryDataSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonBinaryDataSerializer = BsonSerializer.LookupSerializer(typeof(BsonBinaryData));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonBooleanSerializer class.
        /// </summary>
        public static IBsonSerializer BsonBooleanSerializer
        {
            get
            {
                var serializer = __bsonBooleanSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonBooleanSerializer = BsonSerializer.LookupSerializer(typeof(BsonBoolean));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonDateTimeSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDateTimeSerializer
        {
            get
            {
                var serializer = __bsonDateTimeSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonDateTimeSerializer = BsonSerializer.LookupSerializer(typeof(BsonDateTime));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonDocumentSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDocumentSerializer
        {
            get
            {
                var serializer = __bsonDocumentSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonDocumentSerializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonDocumentWrapperSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDocumentWrapperSerializer
        {
            get
            {
                var serializer = __bsonDocumentWrapperSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonDocumentWrapperSerializer = BsonSerializer.LookupSerializer(typeof(BsonDocumentWrapper));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonDoubleSerializer class.
        /// </summary>
        public static IBsonSerializer BsonDoubleSerializer
        {
            get
            {
                var serializer = __bsonDoubleSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonDoubleSerializer = BsonSerializer.LookupSerializer(typeof(BsonDouble));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonInt32Serializer class.
        /// </summary>
        public static IBsonSerializer BsonInt32Serializer
        {
            get
            {
                var serializer = __bsonInt32Serializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonInt32Serializer = BsonSerializer.LookupSerializer(typeof(BsonInt32));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonInt64Serializer class.
        /// </summary>
        public static IBsonSerializer BsonInt64Serializer
        {
            get
            {
                var serializer = __bsonInt64Serializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonInt64Serializer = BsonSerializer.LookupSerializer(typeof(BsonInt64));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonJavaScriptSerializer class.
        /// </summary>
        public static IBsonSerializer BsonJavaScriptSerializer
        {
            get
            {
                var serializer = __bsonJavaScriptSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonJavaScriptSerializer = BsonSerializer.LookupSerializer(typeof(BsonJavaScript));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonJavaScriptWithScopeSerializer class.
        /// </summary>
        public static IBsonSerializer BsonJavaScriptWithScopeSerializer
        {
            get
            {
                var serializer = __bsonJavaScriptWithScopeSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonJavaScriptWithScopeSerializer = BsonSerializer.LookupSerializer(typeof(BsonJavaScriptWithScope));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonMaxKeySerializer class.
        /// </summary>
        public static IBsonSerializer BsonMaxKeySerializer
        {
            get
            {
                var serializer = __bsonMaxKeySerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonMaxKeySerializer = BsonSerializer.LookupSerializer(typeof(BsonMaxKey));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonMinKeySerializer class.
        /// </summary>
        public static IBsonSerializer BsonMinKeySerializer
        {
            get
            {
                var serializer = __bsonMinKeySerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonMinKeySerializer = BsonSerializer.LookupSerializer(typeof(BsonMinKey));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonNullSerializer class.
        /// </summary>
        public static IBsonSerializer BsonNullSerializer
        {
            get
            {
                var serializer = __bsonNullSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonNullSerializer = BsonSerializer.LookupSerializer(typeof(BsonNull));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonObjectIdSerializer class.
        /// </summary>
        public static IBsonSerializer BsonObjectIdSerializer
        {
            get
            {
                var serializer = __bsonObjectIdSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonObjectIdSerializer = BsonSerializer.LookupSerializer(typeof(BsonObjectId));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonRegularExpressionSerializer class.
        /// </summary>
        public static IBsonSerializer BsonRegularExpressionSerializer
        {
            get
            {
                var serializer = __bsonRegularExpressionSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonRegularExpressionSerializer = BsonSerializer.LookupSerializer(typeof(BsonRegularExpression));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonStringSerializer class.
        /// </summary>
        public static IBsonSerializer BsonStringSerializer
        {
            get
            {
                var serializer = __bsonStringSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonStringSerializer = BsonSerializer.LookupSerializer(typeof(BsonString));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonSymbolSerializer class.
        /// </summary>
        public static IBsonSerializer BsonSymbolSerializer
        {
            get
            {
                var serializer = __bsonSymbolSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonSymbolSerializer = BsonSerializer.LookupSerializer(typeof(BsonSymbol));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonTimestampSerializer class.
        /// </summary>
        public static IBsonSerializer BsonTimestampSerializer
        {
            get
            {
                var serializer = __bsonTimestampSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonTimestampSerializer = BsonSerializer.LookupSerializer(typeof(BsonTimestamp));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonUndefinedSerializer class.
        /// </summary>
        public static IBsonSerializer BsonUndefinedSerializer
        {
            get
            {
                var serializer = __bsonUndefinedSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonUndefinedSerializer = BsonSerializer.LookupSerializer(typeof(BsonUndefined));
                    }
                }
                return serializer;
            }
        }

        /// <summary>
        /// Gets an instance of the BsonValueSerializer class.
        /// </summary>
        public static IBsonSerializer BsonValueSerializer
        {
            get
            {
                var serializer = __bsonValueSerializer;
                if (serializer == null)
                {
                    lock (__staticLock)
                    {
                        serializer = __bsonValueSerializer = BsonSerializer.LookupSerializer(typeof(BsonValue));
                    }
                }
                return serializer;
            }
        }
    }
}
