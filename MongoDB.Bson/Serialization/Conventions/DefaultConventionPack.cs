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
    /// Convention pack of defaults.
    /// </summary>
    public class DefaultConventionPack : IConventionPack
    {
        // private fields
        private readonly IEnumerable<IConvention> _conventions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConventionPack" /> class.
        /// </summary>
        public DefaultConventionPack(SerializationContext serializationContext)
        {
            _conventions = new List<IConvention>
            {
                new ReadWriteMemberFinderConvention(),
                new NamedIdMemberConvention(new [] { "Id", "id", "_id" }),
                new NamedExtraElementsMemberConvention(new [] { "ExtraElements" }),
                new IgnoreExtraElementsConvention(false),
                new StringObjectIdIdGeneratorConvention(), // should be before LookupIdGeneratorConvention
                new LookupIdGeneratorConvention(serializationContext)
            };
        }

        // public properties
        /// <summary>
        /// Gets the conventions.
        /// </summary>
        public IEnumerable<IConvention> Conventions
        {
            get { return _conventions; }
        }
    }
}