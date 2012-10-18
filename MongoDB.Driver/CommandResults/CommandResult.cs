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
using System.Text;
using System.Text.RegularExpressions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the result of a command (there are also subclasses for various commands).
    /// </summary>
    [Serializable]
    public class CommandResult
    {
        // private fields
        private readonly SerializationConfig _serializationConfig;
        private readonly IMongoCommand _command;
        private readonly BsonDocument _response;

        // constructors
        /// <summary>
        /// Initializes a new instance of the CommandResult class.
        /// </summary>
        /// <param name="serializationConfig">The serialization config.</param>
        /// <param name="command">The command.</param>
        /// <param name="response">The response.</param>
        public CommandResult(SerializationConfig serializationConfig, IMongoCommand command, BsonDocument response)
        {
            _serializationConfig = serializationConfig;
            _command = command;
            _response = response;
        }

        // public properties
        /// <summary>
        /// Gets the command.
        /// </summary>
        public IMongoCommand Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string CommandName
        {
            get { return _command.ToBsonDocument().GetElement(0).Name; }
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public BsonDocument Response
        {
            get { return _response; }
        }

        /// <summary>
        /// Gets the error message (null if none).
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                BsonValue ok;
                if (_response.TryGetValue("ok", out ok) && ok.ToBoolean())
                {
                    return null;
                }
                else
                {
                    BsonValue errmsg;
                    if (_response.TryGetValue("errmsg", out errmsg) && !errmsg.IsBsonNull)
                    {
                        return errmsg.ToString();
                    }
                    else
                    {
                        return "Unknown error";
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Ok value from the response.
        /// </summary>
        public bool Ok
        {
            get
            {
                BsonValue ok;
                if (_response.TryGetValue("ok", out ok))
                {
                    return ok.ToBoolean();
                }
                else
                {
                    var message = string.Format("Command '{0}' failed. Response has no ok element (response was {1}).", CommandName, _response.ToJson());
                    throw new MongoCommandException(message, this);
                }
            }
        }

        // internal properties
        internal SerializationConfig SerializationConfig
        {
            get { return _serializationConfig; }
        }
    }
}
