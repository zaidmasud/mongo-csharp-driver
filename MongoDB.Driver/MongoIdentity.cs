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
    /// Represents an identity to be used with MongoDB.
    /// </summary>
    public abstract class MongoIdentity
    {
        // public static methods
        /// <summary>
        /// Creates either a floating local identity or an admin local identity.
        /// </summary>
        /// <param name="username">The user name (a "(admin)" suffix indicates an admin local identity).</param>
        /// <returns></returns>
        public static MongoIdentity ParseUsername(string username)
        {
            var admin = false;
            if (username.EndsWith("(admin)", StringComparison.OrdinalIgnoreCase))
            {
                username = username.Substring(0, username.Length - 7);
                admin = true;
            }

            return admin ? (MongoIdentity)new MongoLocalIdentity("admin", username) : new MongoFloatingLocalIdentity(username);
        }

        // public methods
        /// <summary>
        /// Determines whether the specified Identity is equal to the current Identity.
        /// </summary>
        /// <param name="obj">The Identity to compare with the current Identity.</param>
        /// <returns>True if the specified Identity is equal to the current Identity; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            throw new NotImplementedException("Subclasses of MongoIdentity must override Equals.");
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            throw new NotImplementedException("Subclasses of MongoIdentity must override GetHashCode.");
        }
    }
}
