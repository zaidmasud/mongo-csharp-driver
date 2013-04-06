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

using MongoDB.Bson.IO;
using MongoDB.Driver.Internal;

namespace MongoDB.Driver.Operations
{
    internal class RemoveOperation : WriteOperation
    {
        private readonly RemoveFlags _flags;
        private readonly IMongoQuery _query;

        public RemoveOperation(
            string databaseName,
            string collectionName,
            BsonBinaryReaderSettings readerSettings,
            WriteConcern writeConcern,
            BsonBinaryWriterSettings writerSettings,
            IMongoQuery query,
            RemoveFlags flags
            )
            : base(databaseName, collectionName, readerSettings, writeConcern, writerSettings)
        {
            _query = query;
            _flags = flags;
        }

        public WriteConcernResult Execute(MongoConnection connection)
        {
            using (var buffer = new BsonBuffer(new MultiChunkBuffer(BsonChunkPool.Default), true))
            {
                var message = new MongoDeleteMessage(WriterSettings, CollectionFullName, _flags, _query);
                message.WriteToBuffer(buffer);
                if (WriteConcern.Enabled)
                {
                    WriteGetLastErrorMessage(buffer, WriteConcern);
                    connection.SendMessage(message.RequestId, buffer);
                    return ReadWriteConcernResult(connection);
                }
                else
                {
                    connection.SendMessage(message.RequestId, buffer);
                    return null;
                }
            }
        }
    }
}
