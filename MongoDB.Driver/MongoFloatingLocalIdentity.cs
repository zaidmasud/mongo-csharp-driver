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
    /// Represents a local identity that has not yet been associated with a particular database.
    /// </summary>
    public class MongoFloatingLocalIdentity : MongoIdentity
    {
        // private fields
        private readonly string _username;

        // constructors
        /// <summary>
        /// Initializes a new instance of the MongoUsernameIdentity class.
        /// </summary>
        /// <param name="username">The username.</param>
        public MongoFloatingLocalIdentity(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }

            _username = username;
        }

        // public properties
        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username
        {
            get { return _username; }
        }

        // public methods
        /// <summary>
        /// Determines whether the specified Identity is equal to the current Identity.
        /// </summary>
        /// <param name="obj">The Identity to compare with the current Identity.</param>
        /// <returns>True if the specified Identity is equal to the current Identity; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var rhs = (MongoFloatingLocalIdentity)obj;
            if (rhs == null)
            {
                return false; // obj is null or of the wrong type
            }

            return _username == rhs._username;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + _username.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _username;
        }
    }
}
