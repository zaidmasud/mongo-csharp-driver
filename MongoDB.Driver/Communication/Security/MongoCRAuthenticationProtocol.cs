﻿/* Copyright 2010-2013 10gen Inc.
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
using MongoDB.Driver.Internal;
using MongoDB.Driver.Operations;

namespace MongoDB.Driver.Communication.Security
{
    /// <summary>
    /// Authenticates a credential using the MONGODB-CR protocol.
    /// </summary>
    internal class MongoCRAuthenticationProtocol : IAuthenticationProtocol
    {
        // public properties
        public string Name
        {
            get { return "MONGODB-CR"; }
        }

        // public methods
        /// <summary>
        /// Authenticates the connection against the given database.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="credential">The credential.</param>
        public void Authenticate(MongoConnection connection, MongoCredential credential)
        {
            var nonceCommand = new CommandDocument("getnonce", 1);
            var commandResult = CommandOperation<CommandResult>.Create(connection.ServerInstance, credential.Source, nonceCommand, QueryFlags.None).Execute(connection, false);
            if (!commandResult.Ok)
            {
                throw new MongoAuthenticationException(
                    "Error getting nonce for authentication.",
                    new MongoCommandException(commandResult));
            }

            var nonce = commandResult.Response["nonce"].AsString;
            var passwordDigest = ((PasswordEvidence)credential.Evidence).ComputeMongoCRPasswordDigest(credential.Username);
            var digest = MongoUtils.Hash(nonce + credential.Username + passwordDigest);
            var authenticateCommand = new CommandDocument
                {
                    { "authenticate", 1 },
                    { "user", credential.Username },
                    { "nonce", nonce },
                    { "key", digest }
                };

            commandResult = CommandOperation<CommandResult>.Create(connection.ServerInstance, credential.Source, authenticateCommand, QueryFlags.None).Execute(connection, false);
            if (!commandResult.Ok)
            {
                var message = string.Format("Invalid credential for database '{0}'.", credential.Source);
                throw new MongoAuthenticationException(
                    message,
                    new MongoCommandException(commandResult));
            }
        }

        /// <summary>
        /// Determines whether this instance can use the specified credential.
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <returns>
        ///   <c>true</c> if this instance can use the specified credential; otherwise, <c>false</c>.
        /// </returns>
        public bool CanUse(MongoCredential credential)
        {
            return credential.Mechanism.Equals("MONGODB-CR", StringComparison.InvariantCultureIgnoreCase) &&
                credential.Evidence is PasswordEvidence;
        }
    }
}