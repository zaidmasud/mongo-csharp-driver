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
using System.Reflection;
using System.Text;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Convention pack for applying attributes.
    /// </summary>
    public class AttributeConventionPack : IConventionPack
    {
        // private static fields
        private static readonly AttributeConventionPack __attributeConventionPack = new AttributeConventionPack();

        // private fields
        private readonly AttributeConvention _attributeConvention;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeConventionPack" /> class.
        /// </summary>
        public AttributeConventionPack()
        {
            _attributeConvention = new AttributeConvention();
        }

        // public static properties
        /// <summary>
        /// Gets the instance.
        /// </summary>
        [Obsolete]
        public static IConventionPack Instance
        {
            get { return __attributeConventionPack; }
        }

        // public properties
        /// <summary>
        /// Gets the conventions.
        /// </summary>
        public IEnumerable<IConvention> Conventions
        {
            get { yield return _attributeConvention; }
        }

        // nested classes
        private class AttributeConvention : ConventionBase, IClassMapConvention, IMemberMapConvention
        {
            // public methods
            public void Apply(BsonClassMap classMap)
            {
                foreach (IBsonClassMapAttribute attribute in classMap.ClassType.GetCustomAttributes(typeof(IBsonClassMapAttribute), false))
                {
                    attribute.Apply(classMap);
                }

                OptInMembersWithBsonMemberMapModifierAttribute(classMap);
                IgnoreMembersWithBsonIgnoreAttribute(classMap);
                ThrowForDuplicateMemberMapAttributes(classMap);
            }

            public void Apply(BsonMemberMap memberMap)
            {
                foreach (IBsonMemberMapAttribute attribute in memberMap.MemberInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false))
                {
                    attribute.Apply(memberMap);
                }
            }

            // private methods
            private bool AllowsDuplicate(Type type)
            {
                var usageAttribute = type.GetCustomAttributes(typeof(BsonMemberMapAttributeUsageAttribute), true)
                    .OfType<BsonMemberMapAttributeUsageAttribute>()
                    .SingleOrDefault();

                return usageAttribute == null || usageAttribute.AllowMultipleMembers;
            }

            private void OptInMembersWithBsonMemberMapModifierAttribute(BsonClassMap classMap)
            {
                // let other fields opt-in if they have any IBsonMemberMapAttribute attributes
                foreach (var fieldInfo in classMap.ClassType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = fieldInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false).Any();
                    if (hasAttribute)
                    {
                        classMap.MapMember(fieldInfo);
                    }
                }

                // let other properties opt-in if they have any IBsonMemberMapAttribute attributes
                foreach (var propertyInfo in classMap.ClassType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = propertyInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false).Any();
                    if (hasAttribute)
                    {
                        classMap.MapMember(propertyInfo);
                    }
                }
            }

            private void IgnoreMembersWithBsonIgnoreAttribute(BsonClassMap classMap)
            {
                foreach (var memberMap in classMap.DeclaredMemberMaps.ToList())
                {
                    var ignoreAttribute = (BsonIgnoreAttribute)memberMap.MemberInfo.GetCustomAttributes(typeof(BsonIgnoreAttribute), false).FirstOrDefault();
                    if (ignoreAttribute != null)
                    {
                        classMap.UnmapMember(memberMap.MemberInfo);
                    }
                }
            }

            private void ThrowForDuplicateMemberMapAttributes(BsonClassMap classMap)
            {
                var nonDuplicatesAlreadySeen = new List<Type>();
                foreach (var memberMap in classMap.DeclaredMemberMaps)
                {
                    var attributes = (IBsonMemberMapAttribute[])memberMap.MemberInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false);
                    foreach (var attribute in attributes)
                    {
                        var type = attribute.GetType();
                        if (nonDuplicatesAlreadySeen.Contains(type))
                        {
                            var message = string.Format("Attribute of type {0} can only be applied to a single member.", type);
                            throw new DuplicateBsonMemberMapAttributeException(message);
                        }

                        if (!AllowsDuplicate(type))
                        {
                            nonDuplicatesAlreadySeen.Add(type);
                        }
                    }
                }
            }
        }
    }
}