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
    /// Credentials to access a MongoDB database.
    /// </summary>
    [Serializable]
    public class MongoCredentials : IEquatable<MongoCredentials>
    {
        // private fields
        private readonly MongoIdentity _identity;
        private readonly MongoIdentityEvidence _evidence;
        private readonly MongoAuthenticationProtocol _authenticationType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the MongoCredentials class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="evidence">The evidence used to prove the identity.</param>
        /// <param name="authenticationType">The authentication type.</param>
        public MongoCredentials(MongoIdentity identity, MongoIdentityEvidence evidence, MongoAuthenticationProtocol authenticationType)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            _identity = identity;
            _evidence = evidence;
            _authenticationType = authenticationType;
        }

        /// <summary>
        /// Creates a new instance of MongoCredentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        [Obsolete("Use one of the other constructors instead.")]
        public MongoCredentials(string username, string password)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            ValidatePassword(password);

            _identity = MongoIdentity.ParseUsername(username);
            _evidence = new MongoPasswordEvidence(password);
            _authenticationType = MongoAuthenticationProtocol.Original;
        }

        /// <summary>
        /// Creates a new instance of MongoCredentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="admin">Whether the credentials should be validated against the admin database.</param>
        [Obsolete("Use one of the other constructors instead.")]
        public MongoCredentials(string username, string password, bool admin)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            ValidatePassword(password);

            if (username.EndsWith("(admin)", StringComparison.Ordinal))
            {
                username = username.Substring(0, username.Length - 7);
                admin = true;
            }

            _identity = admin ? (MongoIdentity)new MongoLocalIdentity("admin", username) : new MongoFloatingLocalIdentity(username);
            _evidence = new MongoPasswordEvidence(password);
            _authenticationType = MongoAuthenticationProtocol.Original;
        }

        // public properties
        /// <summary>
        /// Gets the authentication type.
        /// </summary>
        public MongoAuthenticationProtocol AuthenticationType
        {
            get { return _authenticationType; }
        }

        /// <summary>
        /// Gets the evidence used to prove the identity.
        /// </summary>
        public MongoIdentityEvidence Evidence
        {
            get { return _evidence; }
        }

        /// <summary>
        /// Gets the identity.
        /// </summary>
        public MongoIdentity Identity
        {
            get { return _identity; }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        [Obsolete("Use Identity instead.")]
        public string Username
        {
            get
            {
                var localIdentity = _identity as MongoLocalIdentity;
                if (localIdentity != null)
                {
                    return localIdentity.Username;
                }

                var floatingLocalIdentity = _identity as MongoFloatingLocalIdentity;
                if (floatingLocalIdentity != null)
                {
                    return floatingLocalIdentity.Username;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        [Obsolete("Use Evidence instead.")]
        public string Password
        {
            get
            {
                var passwordEvidence = _evidence as MongoPasswordEvidence;
                if (passwordEvidence != null)
                {
                    return passwordEvidence.Password;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets whether the credentials should be validated against the admin database.
        /// </summary>
        [Obsolete("Use Identity instead.")]
        public bool Admin
        {
            get
            {
                var localIdentity = _identity as MongoLocalIdentity;
                if (localIdentity != null)
                {
                    return localIdentity.DatabaseName == "admin";
                }

                return false;
            }
        }

        // public operators
        /// <summary>
        /// Compares two MongoCredentials.
        /// </summary>
        /// <param name="lhs">The first MongoCredentials.</param>
        /// <param name="rhs">The other MongoCredentials.</param>
        /// <returns>True if the two MongoCredentials are equal (or both null).</returns>
        public static bool operator ==(MongoCredentials lhs, MongoCredentials rhs)
        {
            return object.Equals(lhs, rhs);
        }

        /// <summary>
        /// Compares two MongoCredentials.
        /// </summary>
        /// <param name="lhs">The first MongoCredentials.</param>
        /// <param name="rhs">The other MongoCredentials.</param>
        /// <returns>True if the two MongoCredentials are not equal (or one is null and the other is not).</returns>
        public static bool operator !=(MongoCredentials lhs, MongoCredentials rhs)
        {
            return !(lhs == rhs);
        }

        // public static methods
        /// <summary>
        /// Creates an instance of MongoCredentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>A new instance of MongoCredentials (or null if either parameter is null).</returns>
        // TODO: should this method be obsoleted?
        [Obsolete("Use one of the constructors instead.")]
        public static MongoCredentials Create(string username, string password)
        {
            if (username != null && password != null)
            {
                return new MongoCredentials(username, password);
            }
            else
            {
                return null;
            }
        }

        // public methods
        /// <summary>
        /// Compares this MongoCredentials to another MongoCredentials.
        /// </summary>
        /// <param name="rhs">The other credentials.</param>
        /// <returns>True if the two credentials are equal.</returns>
        public bool Equals(MongoCredentials rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _identity.Equals(rhs._identity) && object.Equals(_evidence, rhs._evidence) && _authenticationType == rhs._authenticationType;
        }

        /// <summary>
        /// Compares this MongoCredentials to another MongoCredentials.
        /// </summary>
        /// <param name="obj">The other credentials.</param>
        /// <returns>True if the two credentials are equal.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MongoCredentials); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hashcode for the credentials.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + _identity.GetHashCode();
            hash = 37 * hash + ((_evidence == null) ? 0 : _evidence.GetHashCode());
            hash = 37 * hash + _authenticationType.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the credentials.
        /// </summary>
        /// <returns>A string representation of the credentials.</returns>
        public override string ToString()
        {
            if (_evidence == null)
            {
                return _identity.ToString();
            }
            else
            {
                return string.Format("{0}:{1}", _identity, _evidence);
            }
        }

        // private methods
        private void ValidatePassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            if (password.Any(c => (int)c >= 128))
            {
                throw new ArgumentException("Password must contain only ASCII characters.");
            }
        }
    }
}
