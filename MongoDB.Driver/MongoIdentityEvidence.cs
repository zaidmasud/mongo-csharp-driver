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

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents some kind of evidence used to prove an identity.
    /// </summary>
    public abstract class MongoIdentityEvidence
    {
        // public methods
        /// <summary>
        /// Determines whether the specified MongoIdentityEvidence is equal to the current MongoIdentityEvidence.
        /// </summary>
        /// <param name="obj">The MongoIdentityEvidence to compare with the current MongoIdentityEvidence.</param>
        /// <returns>True if the specified MongoIdentityEvidence is equal to the current MongoIdentityEvidence; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            throw new NotImplementedException("Subclasses of MongoIdentityEvidence must override Equals.");
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            throw new NotImplementedException("Subclasses of MongoIdentityEvidence must override GetHashCode.");
        }
    }
}
