﻿/* Copyright 2010-2012 10gen Inc.
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
using System.Threading;

// don't add using statement for MongoDB.Bson.Serialization.Serializers to minimize dependencies on DefaultSerializer
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// A static class that represents the BSON serialization functionality.
    /// </summary>
    public static class BsonSerializer
    {
        // public static properties
        /// <summary>
        /// Gets or sets whether to use the NullIdChecker on reference Id types that don't have an IdGenerator registered.
        /// </summary>
        [Obsolete]
        public static bool UseNullIdChecker
        {
            get { return SerializationConfig.Default.UseNullIdChecker; }
            set { SerializationConfig.Default.UseNullIdChecker = value; }
        }

        /// <summary>
        /// Gets or sets whether to use the ZeroIdChecker on value Id types that don't have an IdGenerator registered.
        /// </summary>
        [Obsolete]
        public static bool UseZeroIdChecker
        {
            get { return SerializationConfig.Default.UseZeroIdChecker; }
            set { SerializationConfig.Default.UseZeroIdChecker = value; }
        }

        // internal static properties
        [Obsolete]
        internal static ReaderWriterLockSlim ConfigLock
        {
            get { return SerializationConfig.Default.ConfigLock; }
        }

        // public static methods
        /// <summary>
        /// Deserializes an object from a BsonDocument.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="document">The BsonDocument.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static TNominalType Deserialize<TNominalType>(BsonDocument document)
        {
            return SerializationConfig.Default.Deserialize<TNominalType>(document);
        }

        /// <summary>
        /// Deserializes an object from a JsonBuffer.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="buffer">The JsonBuffer.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static TNominalType Deserialize<TNominalType>(JsonBuffer buffer)
        {
            return SerializationConfig.Default.Deserialize<TNominalType>(buffer);
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static TNominalType Deserialize<TNominalType>(BsonReader bsonReader)
        {
            return SerializationConfig.Default.Deserialize<TNominalType>(bsonReader);
        }

        /// <summary>
        /// Deserializes an object from a BSON byte array.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bytes">The BSON byte array.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static TNominalType Deserialize<TNominalType>(byte[] bytes)
        {
            return SerializationConfig.Default.Deserialize<TNominalType>(bytes);
        }

        /// <summary>
        /// Deserializes an object from a BSON Stream.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="stream">The BSON Stream.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static TNominalType Deserialize<TNominalType>(Stream stream)
        {
            return SerializationConfig.Default.Deserialize<TNominalType>(stream);
        }

        /// <summary>
        /// Deserializes an object from a JSON string.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static TNominalType Deserialize<TNominalType>(string json)
        {
            return SerializationConfig.Default.Deserialize<TNominalType>(json);
        }

        /// <summary>
        /// Deserializes an object from a JSON TextReader.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="textReader">The JSON TextReader.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static TNominalType Deserialize<TNominalType>(TextReader textReader)
        {
            return SerializationConfig.Default.Deserialize<TNominalType>(textReader);
        }

        /// <summary>
        /// Deserializes an object from a BsonDocument.
        /// </summary>
        /// <param name="document">The BsonDocument.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>A TNominalType.</returns>
        [Obsolete]
        public static object Deserialize(BsonDocument document, Type nominalType)
        {
            return SerializationConfig.Default.Deserialize(document, nominalType);
        }

        /// <summary>
        /// Deserializes an object from a JsonBuffer.
        /// </summary>
        /// <param name="buffer">The JsonBuffer.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        [Obsolete]
        public static object Deserialize(JsonBuffer buffer, Type nominalType)
        {
            return SerializationConfig.Default.Deserialize(buffer, nominalType);
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        [Obsolete]
        public static object Deserialize(BsonReader bsonReader, Type nominalType)
        {
            return SerializationConfig.Default.Deserialize(bsonReader, nominalType);
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        [Obsolete]
        public static object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            return SerializationConfig.Default.Deserialize(bsonReader, nominalType, options);
        }

        /// <summary>
        /// Deserializes an object from a BSON byte array.
        /// </summary>
        /// <param name="bytes">The BSON byte array.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        [Obsolete]
        public static object Deserialize(byte[] bytes, Type nominalType)
        {
            return SerializationConfig.Default.Deserialize(bytes, nominalType);
        }

        /// <summary>
        /// Deserializes an object from a BSON Stream.
        /// </summary>
        /// <param name="stream">The BSON Stream.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        [Obsolete]
        public static object Deserialize(Stream stream, Type nominalType)
        {
            return SerializationConfig.Default.Deserialize(stream, nominalType);
        }

        /// <summary>
        /// Deserializes an object from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        [Obsolete]
        public static object Deserialize(string json, Type nominalType)
        {
            return SerializationConfig.Default.Deserialize(json, nominalType);
        }

        /// <summary>
        /// Deserializes an object from a JSON TextReader.
        /// </summary>
        /// <param name="textReader">The JSON TextReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        [Obsolete]
        public static object Deserialize(TextReader textReader, Type nominalType)
        {
            return SerializationConfig.Default.Deserialize(textReader, nominalType);
        }

        /// <summary>
        /// Returns whether the given type has any discriminators registered for any of its subclasses.
        /// </summary>
        /// <param name="type">A Type.</param>
        /// <returns>True if the type is discriminated.</returns>
        [Obsolete]
        public static bool IsTypeDiscriminated(Type type)
        {
            return SerializationConfig.Default.IsTypeDiscriminated(type);
        }

        /// <summary>
        /// Looks up the actual type of an object to be deserialized.
        /// </summary>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="discriminator">The discriminator.</param>
        /// <returns>The actual type of the object.</returns>
        [Obsolete]
        public static Type LookupActualType(Type nominalType, BsonValue discriminator)
        {
            return SerializationConfig.Default.LookupActualType(nominalType, discriminator);
        }

        /// <summary>
        /// Looks up the discriminator convention for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A discriminator convention.</returns>
        [Obsolete]
        public static IDiscriminatorConvention LookupDiscriminatorConvention(Type type)
        {
            return SerializationConfig.Default.LookupDiscriminatorConvention(type);
        }

        /// <summary>
        /// Looks up a generic serializer definition.
        /// </summary>
        /// <param name="genericTypeDefinition">The generic type.</param>
        /// <returns>A generic serializer definition.</returns>
        [Obsolete]
        public static Type LookupGenericSerializerDefinition(Type genericTypeDefinition)
        {
            return SerializationConfig.Default.LookupGenericSerializerDefinition(genericTypeDefinition);
        }

        /// <summary>
        /// Looks up an IdGenerator.
        /// </summary>
        /// <param name="type">The Id type.</param>
        /// <returns>An IdGenerator for the Id type.</returns>
        [Obsolete]
        public static IIdGenerator LookupIdGenerator(Type type)
        {
            return SerializationConfig.Default.LookupIdGenerator(type);
        }

        /// <summary>
        /// Looks up a serializer for a Type.
        /// </summary>
        /// <param name="type">The Type.</param>
        /// <returns>A serializer for the Type.</returns>
        [Obsolete]
        public static IBsonSerializer LookupSerializer(Type type)
        {
            return SerializationConfig.Default.LookupSerializer(type);
        }

        /// <summary>
        /// Registers the discriminator for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="discriminator">The discriminator.</param>
        [Obsolete]
        public static void RegisterDiscriminator(Type type, BsonValue discriminator)
        {
            SerializationConfig.Default.RegisterDiscriminator(type, discriminator);
        }

        /// <summary>
        /// Registers the discriminator convention for a type.
        /// </summary>
        /// <param name="type">Type type.</param>
        /// <param name="convention">The discriminator convention.</param>
        [Obsolete]
        public static void RegisterDiscriminatorConvention(Type type, IDiscriminatorConvention convention)
        {
            SerializationConfig.Default.RegisterDiscriminatorConvention(type, convention);
        }

        /// <summary>
        /// Registers a generic serializer definition for a generic type.
        /// </summary>
        /// <param name="genericTypeDefinition">The generic type.</param>
        /// <param name="genericSerializerDefinition">The generic serializer definition.</param>
        [Obsolete]
        public static void RegisterGenericSerializerDefinition(
            Type genericTypeDefinition,
            Type genericSerializerDefinition)
        {
            SerializationConfig.Default.RegisterGenericSerializerDefinition(genericTypeDefinition, genericSerializerDefinition);
        }

        /// <summary>
        /// Registers an IdGenerator for an Id Type.
        /// </summary>
        /// <param name="type">The Id Type.</param>
        /// <param name="idGenerator">The IdGenerator for the Id Type.</param>
        [Obsolete]
        public static void RegisterIdGenerator(Type type, IIdGenerator idGenerator)
        {
            SerializationConfig.Default.RegisterIdGenerator(type, idGenerator);
        }

        /// <summary>
        /// Registers a serialization provider.
        /// </summary>
        /// <param name="provider">The serialization provider.</param>
        [Obsolete]
        public static void RegisterSerializationProvider(IBsonSerializationProvider provider)
        {
            SerializationConfig.Default.RegisterSerializationProvider(provider);
        }

        /// <summary>
        /// Registers a serializer for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializer">The serializer.</param>
        [Obsolete]
        public static void RegisterSerializer(Type type, IBsonSerializer serializer)
        {
            SerializationConfig.Default.RegisterSerializer(type, serializer);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="value">The object.</param>
        [Obsolete]
        public static void Serialize<TNominalType>(BsonWriter bsonWriter, TNominalType value)
        {
            SerializationConfig.Default.Serialize<TNominalType>(bsonWriter, value);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        [Obsolete]
        public static void Serialize<TNominalType>(
            BsonWriter bsonWriter,
            TNominalType value,
            IBsonSerializationOptions options)
        {
            SerializationConfig.Default.Serialize<TNominalType>(bsonWriter, value, options);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="value">The object.</param>
        [Obsolete]
        public static void Serialize(BsonWriter bsonWriter, Type nominalType, object value)
        {
            SerializationConfig.Default.Serialize(bsonWriter, nominalType, value);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        [Obsolete]
        public static void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            SerializationConfig.Default.Serialize(bsonWriter, nominalType, value, options);
        }
    }
}
