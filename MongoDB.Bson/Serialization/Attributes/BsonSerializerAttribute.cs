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
            var serializer = CreateSerializer(memberMap.ClassMap.SerializationContext, memberMap.MemberType);
            memberMap.SetSerializer(serializer);
        }

        /// <summary>
        /// Creates a serializer for a type based on the serializer type specified by the attribute.
        /// </summary>
        /// <param name="serializationContext">The serialization context.</param>
        /// <param name="type">The type that a serializer should be created for.</param>
        /// <returns>A serializer for the type.</returns>
        internal IBsonSerializer CreateSerializer(SerializationContext serializationContext, Type type)
        {
            string message;

            if (type.ContainsGenericParameters)
            {
                message = "Cannot create a serializer because the type to serialize is an open generic type.";
                throw new ArgumentException(message);
            }

            if (_serializerType.ContainsGenericParameters)
            {
                message = "Cannot create a serializer because the serializer type is an open generic type.";
                throw new ArgumentException(message);
            }

            var constructorInfo = _serializerType.GetConstructor(new[] { typeof(SerializationContext) });
            if (constructorInfo != null)
            {
                return (IBsonSerializer)constructorInfo.Invoke(new[] { serializationContext });
            }

            constructorInfo = _serializerType.GetConstructor(new Type[0]);
            if (constructorInfo != null)
            {
                if (serializationContext != SerializationContext.Default)
                {
                    message = string.Format("Serializer type {0} no-argument constructor can only be used with the default serialization context.", _serializerType.FullName);
                    throw new ArgumentException(message);
                }
                return (IBsonSerializer)constructorInfo.Invoke(new object[0]);
            }

            message = string.Format("No suitable constructor found for serializer type {0}.", _serializerType.FullName);
            throw new ArgumentException(message);
        }
    }
}
