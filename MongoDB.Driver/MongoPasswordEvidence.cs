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

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a password used to prove an identity.
    /// </summary>
    public class MongoPasswordEvidence : MongoIdentityEvidence
    {
        // private fields
        private readonly string _password; // TODO: use SecureString

        // constructors
        /// <summary>
        /// Initializes a new instance of the MongoPasswordEvidence class.
        /// </summary>
        /// <param name="password"></param>
        public MongoPasswordEvidence(string password)
        {
            _password = password;
        }

        // public properties
        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password
        {
            get { return _password; }
        }

        // public methods
        /// <summary>
        /// Determines whether the specified MongoPasswordEvidence is equal to the current MongoPasswordEvidence.
        /// </summary>
        /// <param name="obj">The MongoPasswordEvidence to compare with the current MongoPasswordEvidence.</param>
        /// <returns>True if the specified MongoPasswordEvidence is equal to the current MongoPasswordEvidence; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var rhs = (MongoPasswordEvidence)obj;
            if (rhs == null)
            {
                return false; // obj is null or of the wrong type
            }

            return _password == rhs._password;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return _password.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _password;
        }
    }
}
