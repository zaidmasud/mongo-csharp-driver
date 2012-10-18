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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// A class backed by a BsonDocument.
    /// </summary>
    public abstract class BsonDocumentBackedClass
    {
        // private fields
        private readonly SerializationConfig _serializationConfig;
        private readonly IBsonDocumentSerializer _serializer;
        private readonly BsonDocument _backingDocument;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentBackedClass"/> class.
        /// </summary>
        /// <param name="serializationConfig">The serialization config.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="backingDocument">The backing document.</param>
        protected BsonDocumentBackedClass(SerializationConfig serializationConfig, IBsonDocumentSerializer serializer, BsonDocument backingDocument)
        {
            if (serializationConfig == null)
            {
                throw new ArgumentNullException("serializationConfig");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            if (backingDocument == null)
            {
                throw new ArgumentNullException("backingDocument");
            }

            _serializationConfig = serializationConfig;
            _serializer = serializer;
            _backingDocument = backingDocument;
        }

        // protected internal properties
        /// <summary>
        /// Gets the backing document.
        /// </summary>
        protected internal BsonDocument BackingDocument
        {
            get { return _backingDocument; }
        }

        // protected methods
        /// <summary>
        /// Gets the value from the backing document.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="memberName">The member name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        protected T GetValue<T>(string memberName, T defaultValue)
        {
            var info = _serializer.GetMemberSerializationInfo(_serializationConfig, memberName);

            BsonValue bsonValue;
            if (!_backingDocument.TryGetValue(info.ElementName, out bsonValue))
            {
                return defaultValue;
            }

            return (T)info.DeserializeValue(bsonValue);
        }

        /// <summary>
        /// Sets the value in the backing document.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <param name="value">The value.</param>
        protected void SetValue(string memberName, object value)
        {
            var info = _serializer.GetMemberSerializationInfo(_serializationConfig, memberName);
            var bsonValue = info.SerializeValue(value);
            _backingDocument.Set(info.ElementName, bsonValue);
        }
    }
}