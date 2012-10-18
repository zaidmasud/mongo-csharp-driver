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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Specifies the type of the serializer to use for a class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonSerializerAttribute : Attribute, IBsonMemberMapAttribute
    {
        // private fields
        private Type _serializerType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonSerializerAttribute class.
        /// </summary>
        public BsonSerializerAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonSerializerAttribute class.
        /// </summary>
        /// <param name="serializerType">The type of the serializer to use for a class.</param>
        public BsonSerializerAttribute(Type serializerType)
        {
            _serializerType = serializerType;
        }

        // public properties
        /// <summary>
        /// Gets or sets the type of the serializer to use for a class.
        /// </summary>
        public Type SerializerType
        {
            get { return _serializerType; }
            set { _serializerType = value; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            var serializationConfig = memberMap.ClassMap.SerializationConfig;
            var serializer = CreateSerializer(serializationConfig);
            memberMap.SetSerializer(serializer);
        }

        /// <summary>
        /// Creates a serializer based on the serializer type specified by the attribute.
        /// </summary>
        /// <param name="serializationConfig">The serialization config.</param>
        /// <returns>A serializer.</returns>
        internal IBsonSerializer CreateSerializer(SerializationConfig serializationConfig)
        {
            return BsonDefaultSerializationProvider.CreateSerializer(serializationConfig, _serializerType);
        }
    }
}
