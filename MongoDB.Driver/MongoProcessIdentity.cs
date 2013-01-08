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
using System.Linq;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the identity of the current process.
    /// </summary>
    public class MongoProcessIdentity : MongoIdentity
    {
        // public methods
        /// <summary>
        /// Determines whether the specified Identity is equal to the current Identity.
        /// </summary>
        /// <param name="obj">The Identity to compare with the current Identity.</param>
        /// <returns>True if the specified Identity is equal to the current Identity; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var rhs = (MongoProcessIdentity)obj;
            if (rhs == null)
            {
                return false; // obj is null or of the wrong type
            }

            return true; // all instances of MongoProcessIdentity are considered equal to each other
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
