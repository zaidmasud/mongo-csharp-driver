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
using System.Threading;

// don't add using statement for MongoDB.Bson.Serialization.Serializers to minimize dependencies on DefaultSerializer
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// A class that represents a BSON serialization config and its settings.
    /// </summary>
    public class SerializationConfig
    {
        // private static fields
        private static readonly SerializationConfig __default;

        // private fields
        private readonly string _name;
        private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<Type, IIdGenerator> _idGenerators = new Dictionary<Type, IIdGenerator>();
        private readonly Dictionary<Type, IBsonSerializer> _serializers = new Dictionary<Type, IBsonSerializer>();
        private readonly Dictionary<Type, Type> _genericSerializerDefinitions = new Dictionary<Type, Type>();
        private readonly List<IBsonSerializationProvider> _serializationProviders = new List<IBsonSerializationProvider>();
        private readonly Dictionary<Type, IDiscriminatorConvention> _discriminatorConventions = new Dictionary<Type, IDiscriminatorConvention>();
        private readonly Dictionary<BsonValue, HashSet<Type>> _discriminators = new Dictionary<BsonValue, HashSet<Type>>();
        private readonly HashSet<Type> _discriminatedTypes = new HashSet<Type>();
        private readonly HashSet<Type> _typesWithRegisteredKnownTypes = new HashSet<Type>();
        private readonly Dictionary<Type, BsonClassMap> _classMaps = new Dictionary<Type, BsonClassMap>();
        private readonly ConventionRegistry _conventionRegistry;

        private bool _useNullIdChecker = false;
        private bool _useZeroIdChecker = false;

        // static constructor
        static SerializationConfig()
        {
            __default = new SerializationConfig("default");
            __default.RegisterDefaultSerializationProviders();
            __default.RegisterIdGenerators();
        }

        // constructors
        /// <summary>
        /// Initializes a new instance of the SerializationConfig class.
        /// </summary>
        /// <param name="name">The name of the serialization config.</param>
        public SerializationConfig(string name)
        {
            _name = name;
            _conventionRegistry = new ConventionRegistry(this);
        }

        // public static properties
        /// <summary>
        /// Gets the default serialization config.
        /// </summary>
        public static SerializationConfig Default
        {
            get { return __default; }
        }

        // public properties
        /// <summary>
        /// Gets the name of the serialization config.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets or sets whether to use the NullIdChecker on reference Id types that don't have an IdGenerator registered.
        /// </summary>
        public bool UseNullIdChecker
        {
            get { return _useNullIdChecker; }
            set { _useNullIdChecker = value; }
        }

        /// <summary>
        /// Gets or sets whether to use the ZeroIdChecker on value Id types that don't have an IdGenerator registered.
        /// </summary>
        public bool UseZeroIdChecker
        {
            get { return _useZeroIdChecker; }
            set { _useZeroIdChecker = value; }
        }

        // internal properties
        internal ReaderWriterLockSlim ConfigLock
        {
            get { return _configLock; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonDocument.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="document">The BsonDocument.</param>
        /// <returns>A TNominalType.</returns>
        public TNominalType Deserialize<TNominalType>(BsonDocument document)
        {
            return (TNominalType)Deserialize(document, typeof(TNominalType));
        }

        /// <summary>
        /// Deserializes an object from a JsonBuffer.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="buffer">The JsonBuffer.</param>
        /// <returns>A TNominalType.</returns>
        public TNominalType Deserialize<TNominalType>(JsonBuffer buffer)
        {
            return (TNominalType)Deserialize(buffer, typeof(TNominalType));
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <returns>A TNominalType.</returns>
        public TNominalType Deserialize<TNominalType>(BsonReader bsonReader)
        {
            return (TNominalType)Deserialize(bsonReader, typeof(TNominalType));
        }

        /// <summary>
        /// Deserializes an object from a BSON byte array.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bytes">The BSON byte array.</param>
        /// <returns>A TNominalType.</returns>
        public TNominalType Deserialize<TNominalType>(byte[] bytes)
        {
            return (TNominalType)Deserialize(bytes, typeof(TNominalType));
        }

        /// <summary>
        /// Deserializes an object from a BSON Stream.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="stream">The BSON Stream.</param>
        /// <returns>A TNominalType.</returns>
        public TNominalType Deserialize<TNominalType>(Stream stream)
        {
            return (TNominalType)Deserialize(stream, typeof(TNominalType));
        }

        /// <summary>
        /// Deserializes an object from a JSON string.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>A TNominalType.</returns>
        public TNominalType Deserialize<TNominalType>(string json)
        {
            return (TNominalType)Deserialize(json, typeof(TNominalType));
        }

        /// <summary>
        /// Deserializes an object from a JSON TextReader.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="textReader">The JSON TextReader.</param>
        /// <returns>A TNominalType.</returns>
        public TNominalType Deserialize<TNominalType>(TextReader textReader)
        {
            return (TNominalType)Deserialize(textReader, typeof(TNominalType));
        }

        /// <summary>
        /// Deserializes an object from a BsonDocument.
        /// </summary>
        /// <param name="document">The BsonDocument.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>A TNominalType.</returns>
        public object Deserialize(BsonDocument document, Type nominalType)
        {
            using (var bsonReader = BsonReader.Create(document))
            {
                return Deserialize(bsonReader, nominalType);
            }
        }

        /// <summary>
        /// Deserializes an object from a JsonBuffer.
        /// </summary>
        /// <param name="buffer">The JsonBuffer.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        public object Deserialize(JsonBuffer buffer, Type nominalType)
        {
            using (var bsonReader = BsonReader.Create(buffer))
            {
                return Deserialize(bsonReader, nominalType);
            }
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        public object Deserialize(BsonReader bsonReader, Type nominalType)
        {
            return Deserialize(bsonReader, nominalType, null);
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            // if nominalType is an interface find out the actualType and use it instead
            if (nominalType.IsInterface)
            {
                var discriminatorConvention = LookupDiscriminatorConvention(nominalType);
                var actualType = discriminatorConvention.GetActualType(this, bsonReader, nominalType);
                if (actualType == nominalType)
                {
                    var message = string.Format("Unable to determine actual type of object to deserialize. NominalType is the interface {0}.", nominalType);
                    throw new FileFormatException(message);
                }
                var serializer = LookupSerializer(actualType);
                return serializer.Deserialize(this, bsonReader, actualType, options);
            }
            else
            {
                var serializer = LookupSerializer(nominalType);
                return serializer.Deserialize(this, bsonReader, nominalType, options);
            }
        }

        /// <summary>
        /// Deserializes an object from a BSON byte array.
        /// </summary>
        /// <param name="bytes">The BSON byte array.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        public object Deserialize(byte[] bytes, Type nominalType)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                return Deserialize(memoryStream, nominalType);
            }
        }

        /// <summary>
        /// Deserializes an object from a BSON Stream.
        /// </summary>
        /// <param name="stream">The BSON Stream.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        public object Deserialize(Stream stream, Type nominalType)
        {
            using (var bsonReader = BsonReader.Create(stream))
            {
                return Deserialize(bsonReader, nominalType);
            }
        }

        /// <summary>
        /// Deserializes an object from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        public object Deserialize(string json, Type nominalType)
        {
            using (var bsonReader = BsonReader.Create(json))
            {
                return Deserialize(bsonReader, nominalType);
            }
        }

        /// <summary>
        /// Deserializes an object from a JSON TextReader.
        /// </summary>
        /// <param name="textReader">The JSON TextReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>An object.</returns>
        public object Deserialize(TextReader textReader, Type nominalType)
        {
            using (var bsonReader = BsonReader.Create(textReader))
            {
                return Deserialize(bsonReader, nominalType);
            }
        }

        /// <summary>
        /// Checks whether a class map is registered for a type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if there is a class map registered for the type.</returns>
        public bool IsClassMapRegistered(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            _configLock.EnterReadLock();
            try
            {
                return _classMaps.ContainsKey(type);
            }
            finally
            {
                _configLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns whether the given type has any discriminators registered for any of its subclasses.
        /// </summary>
        /// <param name="type">A Type.</param>
        /// <returns>True if the type is discriminated.</returns>
        public bool IsTypeDiscriminated(Type type)
        {
            return type.IsInterface || _discriminatedTypes.Contains(type);
        }

        /// <summary>
        /// Looks up the actual type of an object to be deserialized.
        /// </summary>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="discriminator">The discriminator.</param>
        /// <returns>The actual type of the object.</returns>
        public Type LookupActualType(Type nominalType, BsonValue discriminator)
        {
            if (discriminator == null)
            {
                return nominalType;
            }

            // note: EnsureKnownTypesAreRegistered handles its own locking so call from outside any lock
            EnsureKnownTypesAreRegistered(nominalType);

            _configLock.EnterReadLock();
            try
            {
                Type actualType = null;

                HashSet<Type> hashSet;
                if (_discriminators.TryGetValue(discriminator, out hashSet))
                {
                    foreach (var type in hashSet)
                    {
                        if (nominalType.IsAssignableFrom(type))
                        {
                            if (actualType == null)
                            {
                                actualType = type;
                            }
                            else
                            {
                                string message = string.Format("Ambiguous discriminator '{0}'.", discriminator);
                                throw new BsonSerializationException(message);
                            }
                        }
                    }
                }

                if (actualType == null && discriminator.IsString)
                {
                    actualType = TypeNameDiscriminator.GetActualType(discriminator.AsString); // see if it's a Type name
                }

                if (actualType == null)
                {
                    string message = string.Format("Unknown discriminator value '{0}'.", discriminator);
                    throw new BsonSerializationException(message);
                }

                if (!nominalType.IsAssignableFrom(actualType))
                {
                    string message = string.Format(
                        "Actual type {0} is not assignable to expected type {1}.",
                        actualType.FullName, nominalType.FullName);
                    throw new BsonSerializationException(message);
                }

                return actualType;
            }
            finally
            {
                _configLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Looks up a class map (will AutoMap the class if no class map is registered).
        /// </summary>
        /// <param name="classType">The class type.</param>
        /// <returns>The class map.</returns>
        public BsonClassMap LookupClassMap(Type classType)
        {
            if (classType == null)
            {
                throw new ArgumentNullException("classType");
            }

            _configLock.EnterReadLock();
            try
            {
                BsonClassMap classMap;
                if (_classMaps.TryGetValue(classType, out classMap))
                {
                    if (classMap.IsFrozen)
                    {
                        return classMap;
                    }
                }
            }
            finally
            {
                _configLock.ExitReadLock();
            }

            _configLock.EnterWriteLock();
            try
            {
                BsonClassMap classMap;
                if (!_classMaps.TryGetValue(classType, out classMap))
                {
                    // automatically create a classMap for classType and register it
                    var classMapDefinition = typeof(BsonClassMap<>);
                    var classMapType = classMapDefinition.MakeGenericType(classType);
                    classMap = (BsonClassMap)Activator.CreateInstance(classMapType, this);
                    classMap.AutoMap();
                    RegisterClassMap(classMap);
                }
                return classMap.Freeze();
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        // public methods
        /// <summary>
        /// Looks up the effective set of conventions that apply to a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The conventions for that type.</returns>
        public IConventionPack LookupConventions(Type type)
        {
            return _conventionRegistry.LookupConventions(type);
        }

        /// <summary>
        /// Looks up the discriminator convention for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A discriminator convention.</returns>
        public IDiscriminatorConvention LookupDiscriminatorConvention(Type type)
        {
            _configLock.EnterReadLock();
            try
            {
                IDiscriminatorConvention convention;
                if (_discriminatorConventions.TryGetValue(type, out convention))
                {
                    return convention;
                }
            }
            finally
            {
                _configLock.ExitReadLock();
            }

            _configLock.EnterWriteLock();
            try
            {
                IDiscriminatorConvention convention;
                if (!_discriminatorConventions.TryGetValue(type, out convention))
                {
                    // if there is no convention registered for object register the default one
                    if (!_discriminatorConventions.ContainsKey(typeof(object)))
                    {
                        var defaultDiscriminatorConvention = StandardDiscriminatorConvention.Hierarchical;
                        _discriminatorConventions.Add(typeof(object), defaultDiscriminatorConvention);
                        if (type == typeof(object))
                        {
                            return defaultDiscriminatorConvention;
                        }
                    }

                    if (type.IsInterface)
                    {
                        // TODO: should convention for interfaces be inherited from parent interfaces?
                        convention = _discriminatorConventions[typeof(object)];
                        _discriminatorConventions[type] = convention;
                    }
                    else
                    {
                        // inherit the discriminator convention from the closest parent that has one
                        Type parentType = type.BaseType;
                        while (convention == null)
                        {
                            if (parentType == null)
                            {
                                var message = string.Format("No discriminator convention found for type {0}.", type.FullName);
                                throw new BsonSerializationException(message);
                            }
                            if (_discriminatorConventions.TryGetValue(parentType, out convention))
                            {
                                break;
                            }
                            parentType = parentType.BaseType;
                        }

                        // register this convention for all types between this and the parent type where we found the convention
                        var unregisteredType = type;
                        while (unregisteredType != parentType)
                        {
                            RegisterDiscriminatorConvention(unregisteredType, convention);
                            unregisteredType = unregisteredType.BaseType;
                        }
                    }
                }
                return convention;
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Looks up a generic serializer definition.
        /// </summary>
        /// <param name="genericTypeDefinition">The generic type.</param>
        /// <returns>A generic serializer definition.</returns>
        public Type LookupGenericSerializerDefinition(Type genericTypeDefinition)
        {
            _configLock.EnterReadLock();
            try
            {
                Type genericSerializerDefinition;
                _genericSerializerDefinitions.TryGetValue(genericTypeDefinition, out genericSerializerDefinition);
                return genericSerializerDefinition;
            }
            finally
            {
                _configLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Looks up an IdGenerator.
        /// </summary>
        /// <param name="type">The Id type.</param>
        /// <returns>An IdGenerator for the Id type.</returns>
        public IIdGenerator LookupIdGenerator(Type type)
        {
            _configLock.EnterReadLock();
            try
            {
                IIdGenerator idGenerator;
                if (_idGenerators.TryGetValue(type, out idGenerator))
                {
                    return idGenerator;
                }
            }
            finally
            {
                _configLock.ExitReadLock();
            }

            _configLock.EnterWriteLock();
            try
            {
                IIdGenerator idGenerator;
                if (!_idGenerators.TryGetValue(type, out idGenerator))
                {
                    if (type.IsValueType && _useZeroIdChecker)
                    {
                        var iEquatableDefinition = typeof(IEquatable<>);
                        var iEquatableType = iEquatableDefinition.MakeGenericType(type);
                        if (iEquatableType.IsAssignableFrom(type))
                        {
                            var zeroIdCheckerDefinition = typeof(ZeroIdChecker<>);
                            var zeroIdCheckerType = zeroIdCheckerDefinition.MakeGenericType(type);
                            idGenerator = (IIdGenerator)Activator.CreateInstance(zeroIdCheckerType);
                        }
                    }
                    else if (_useNullIdChecker)
                    {
                        idGenerator = NullIdChecker.Instance;
                    }
                    else
                    {
                        idGenerator = null;
                    }

                    _idGenerators[type] = idGenerator; // remember it even if it's null
                }

                return idGenerator;
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Looks up a serializer for a Type.
        /// </summary>
        /// <param name="type">The Type.</param>
        /// <returns>A serializer for the Type.</returns>
        public IBsonSerializer LookupSerializer(Type type)
        {
            _configLock.EnterReadLock();
            try
            {
                IBsonSerializer serializer;
                if (_serializers.TryGetValue(type, out serializer))
                {
                    return serializer;
                }
            }
            finally
            {
                _configLock.ExitReadLock();
            }

            _configLock.EnterWriteLock();
            try
            {
                IBsonSerializer serializer;
                if (!_serializers.TryGetValue(type, out serializer))
                {
                    if (serializer == null)
                    {
                        var serializerAttributes = type.GetCustomAttributes(typeof(BsonSerializerAttribute), false); // don't inherit
                        if (serializerAttributes.Length == 1)
                        {
                            var serializerAttribute = (BsonSerializerAttribute)serializerAttributes[0];
                            serializer = serializerAttribute.CreateSerializer(this);
                        }
                    }

                    if (serializer == null && type.IsGenericType)
                    {
                        var genericTypeDefinition = type.GetGenericTypeDefinition();
                        var genericSerializerDefinition = LookupGenericSerializerDefinition(genericTypeDefinition);
                        if (genericSerializerDefinition != null)
                        {
                            var genericSerializerType = genericSerializerDefinition.MakeGenericType(type.GetGenericArguments());
                            serializer = (IBsonSerializer)Activator.CreateInstance(genericSerializerType, this);
                        }
                    }

                    if (serializer == null)
                    {
                        foreach (var serializationProvider in _serializationProviders)
                        {
                            serializer = serializationProvider.GetSerializer(this, type);
                            if (serializer != null)
                            {
                                break;
                            }
                        }
                    }

                    if (serializer == null)
                    {
                        var message = string.Format("No serializer found for type {0}.", type.FullName);
                        throw new BsonSerializationException(message);
                    }

                    _serializers[type] = serializer;
                }

                return serializer;
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Creates and registers a class map.
        /// </summary>
        /// <typeparam name="TClass">The class.</typeparam>
        /// <returns>The class map.</returns>
        public BsonClassMap<TClass> RegisterClassMap<TClass>()
        {
            return RegisterClassMap<TClass>(cm => { cm.AutoMap(); });
        }

        /// <summary>
        /// Creates and registers a class map.
        /// </summary>
        /// <typeparam name="TClass">The class.</typeparam>
        /// <param name="classMapInitializer">The class map initializer.</param>
        /// <returns>The class map.</returns>
        public BsonClassMap<TClass> RegisterClassMap<TClass>(Action<BsonClassMap<TClass>> classMapInitializer)
        {
            var classMap = new BsonClassMap<TClass>(this, classMapInitializer);
            RegisterClassMap(classMap);
            return classMap;
        }

        /// <summary>
        /// Registers a class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void RegisterClassMap(BsonClassMap classMap)
        {
            if (classMap == null)
            {
                throw new ArgumentNullException("classMap");
            }

            _configLock.EnterWriteLock();
            try
            {
                // note: class maps can NOT be replaced (because derived classes refer to existing instance)
                _classMaps.Add(classMap.ClassType, classMap);
                RegisterDiscriminator(classMap.ClassType, classMap.Discriminator);
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers the conventions.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="conventions">The conventions.</param>
        /// <param name="filter">The filter.</param>
        public void RegisterConventions(string name, IConventionPack conventions, Func<Type, bool> filter)
        {
            _conventionRegistry.RegisterConventions(name, conventions, filter);
        }

        /// <summary>
        /// Registers the discriminator for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="discriminator">The discriminator.</param>
        public void RegisterDiscriminator(Type type, BsonValue discriminator)
        {
            if (type.IsInterface)
            {
                var message = string.Format("Discriminators can only be registered for classes, not for interface {0}.", type.FullName);
                throw new BsonSerializationException(message);
            }

            _configLock.EnterWriteLock();
            try
            {
                HashSet<Type> hashSet;
                if (!_discriminators.TryGetValue(discriminator, out hashSet))
                {
                    hashSet = new HashSet<Type>();
                    _discriminators.Add(discriminator, hashSet);
                }

                if (!hashSet.Contains(type))
                {
                    hashSet.Add(type);

                    // mark all base types as discriminated (so we know that it's worth reading a discriminator)
                    for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
                    {
                        _discriminatedTypes.Add(baseType);
                    }
                }
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers the discriminator convention for a type.
        /// </summary>
        /// <param name="type">Type type.</param>
        /// <param name="convention">The discriminator convention.</param>
        public void RegisterDiscriminatorConvention(Type type, IDiscriminatorConvention convention)
        {
            _configLock.EnterWriteLock();
            try
            {
                if (!_discriminatorConventions.ContainsKey(type))
                {
                    _discriminatorConventions.Add(type, convention);
                }
                else
                {
                    var message = string.Format("There is already a discriminator convention registered for type {0}.", type.FullName);
                    throw new BsonSerializationException(message);
                }
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers a generic serializer definition for a generic type.
        /// </summary>
        /// <param name="genericTypeDefinition">The generic type.</param>
        /// <param name="genericSerializerDefinition">The generic serializer definition.</param>
        public void RegisterGenericSerializerDefinition(
            Type genericTypeDefinition,
            Type genericSerializerDefinition)
        {
            _configLock.EnterWriteLock();
            try
            {
                _genericSerializerDefinitions[genericTypeDefinition] = genericSerializerDefinition;
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers an IdGenerator for an Id Type.
        /// </summary>
        /// <param name="type">The Id Type.</param>
        /// <param name="idGenerator">The IdGenerator for the Id Type.</param>
        public void RegisterIdGenerator(Type type, IIdGenerator idGenerator)
        {
            _configLock.EnterWriteLock();
            try
            {
                _idGenerators[type] = idGenerator;
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers a serialization provider.
        /// </summary>
        /// <param name="provider">The serialization provider.</param>
        public void RegisterSerializationProvider(IBsonSerializationProvider provider)
        {
            _configLock.EnterWriteLock();
            try
            {
                // add new provider to the front of the list
                _serializationProviders.Insert(0, provider);
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers a serializer for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializer">The serializer.</param>
        public void RegisterSerializer(Type type, IBsonSerializer serializer)
        {
            // don't allow any serializers to be registered for subclasses of BsonDocument
            if (typeof(BsonDocument).IsAssignableFrom(type))
            {
                var message = string.Format("A serializer cannot be registered for type {0} because it is a subclass of BsonDocument.", BsonUtils.GetFriendlyTypeName(type));
                throw new BsonSerializationException(message);
            }

            if (typeof(IBsonSerializable).IsAssignableFrom(type))
            {
                var message = string.Format("A serializer cannot be registered for type {0} because it implements IBsonSerializable.", BsonUtils.GetFriendlyTypeName(type));
                throw new BsonSerializationException(message);
            }

            _configLock.EnterWriteLock();
            try
            {
                if (_serializers.ContainsKey(type))
                {
                    var message = string.Format("There is already a serializer registered for type {0}.", type.FullName);
                    throw new BsonSerializationException(message);
                }
                _serializers.Add(type, serializer);
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes the conventions specified by the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks>Removing a convention allows the removal of the special __defaults__ conventions 
        /// and the __attributes__ conventions for those who want to completely customize the 
        /// experience.</remarks>
        public void RemoveConventions(string name)
        {
            _conventionRegistry.RemoveConventions(name);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="value">The object.</param>
        public void Serialize<TNominalType>(BsonWriter bsonWriter, TNominalType value)
        {
            Serialize(bsonWriter, value, null);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public void Serialize<TNominalType>(
            BsonWriter bsonWriter,
            TNominalType value,
            IBsonSerializationOptions options)
        {
            Serialize(bsonWriter, typeof(TNominalType), value, options);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="value">The object.</param>
        public void Serialize(BsonWriter bsonWriter, Type nominalType, object value)
        {
            Serialize(bsonWriter, nominalType, value, null);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            var actualType = (value == null) ? nominalType : value.GetType();
            var serializer = LookupSerializer(actualType);
            serializer.Serialize(this, bsonWriter, nominalType, value, options);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _name;
        }

        // internal methods
        internal void EnsureKnownTypesAreRegistered(Type nominalType)
        {
            _configLock.EnterReadLock();
            try
            {
                if (_typesWithRegisteredKnownTypes.Contains(nominalType))
                {
                    return;
                }
            }
            finally
            {
                _configLock.ExitReadLock();
            }

            _configLock.EnterWriteLock();
            try
            {
                if (!_typesWithRegisteredKnownTypes.Contains(nominalType))
                {
                    // only call LookupClassMap for classes with a BsonKnownTypesAttribute
                    var knownTypesAttribute = nominalType.GetCustomAttributes(typeof(BsonKnownTypesAttribute), false);
                    if (knownTypesAttribute != null && knownTypesAttribute.Length > 0)
                    {
                        // try and force a scan of the known types
                        LookupSerializer(nominalType);
                    }

                    _typesWithRegisteredKnownTypes.Add(nominalType);
                }
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        // private methods
        private void RegisterDefaultSerializationProviders()
        {
            // last one registered gets first chance at providing the serializer
            RegisterSerializationProvider(new BsonClassMapSerializationProvider());
            RegisterSerializationProvider(new BsonDefaultSerializationProvider());
        }

        private void RegisterIdGenerators()
        {
            RegisterIdGenerator(typeof(BsonObjectId), BsonObjectIdGenerator.Instance);
            RegisterIdGenerator(typeof(Guid), GuidGenerator.Instance);
            RegisterIdGenerator(typeof(ObjectId), ObjectIdGenerator.Instance);
        }
    }
}
