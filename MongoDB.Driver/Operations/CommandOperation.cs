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

using System.Collections.Generic;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Internal;
using MongoDB.Bson;

namespace MongoDB.Driver.Operations
{
    internal class CommandOperation<TCommandResult> : DatabaseOperation where TCommandResult : CommandResult
    {
        private readonly IMongoCommand _command;
        private readonly QueryFlags _flags;
        private readonly BsonDocument _options;
        private readonly ReadPreference _readPreference;
        private readonly IBsonSerializationOptions _serializationOptions;
        private readonly IBsonSerializer _serializer;

        public CommandOperation(
            string databaseName,
            BsonBinaryReaderSettings readerSettings,
            BsonBinaryWriterSettings writerSettings,
            IMongoCommand command,
            QueryFlags flags,
            BsonDocument options,
            ReadPreference readPreference,
            IBsonSerializationOptions serializationOptions,
            IBsonSerializer serializer)
            : base(databaseName, "$cmd", readerSettings, writerSettings)
        {
            _command = command;
            _flags = flags;
            _options = options;
            _readPreference = readPreference;
            _serializationOptions = serializationOptions;
            _serializer = serializer;
        }

        public TCommandResult Execute(MongoConnection connection, bool throwOnError)
        {
            var readerSettings = GetNodeAdjustedReaderSettings(connection.ServerInstance);
            var writerSettings = GetNodeAdjustedWriterSettings(connection.ServerInstance);
            var forShardRouter = connection.ServerInstance.InstanceType == MongoServerInstanceType.ShardRouter;
            var wrappedQuery = QueryWrapper.WrapQuery(_command, _options, _readPreference, forShardRouter);
            var queryMessage = new MongoQueryMessage(writerSettings, CollectionFullName, _flags, 0, -1, wrappedQuery, null);
            connection.SendMessage(queryMessage);

            var reply = connection.ReceiveMessage<TCommandResult>(readerSettings, _serializer, _serializationOptions);
            var commandResult = reply.Documents[0];
            commandResult.Command = _command;

            if (throwOnError && !commandResult.Ok)
            {
                throw new MongoCommandException(commandResult);
            }

            return commandResult;
        }
    }
}
