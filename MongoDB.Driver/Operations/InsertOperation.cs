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
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Internal;

namespace MongoDB.Driver.Operations
{
    internal class InsertOperation : WriteOperation
    {
        private readonly bool _assignIdOnInsert;
        private readonly bool _checkElementNames;
        private readonly Type _documentType;
        private readonly IEnumerable _documents;
        private readonly InsertFlags _flags;

        public InsertOperation(
            string databaseName,
            string collectionName,
            BsonBinaryReaderSettings readerSettings,
            BsonBinaryWriterSettings writerSettings,
            WriteConcern writeConcern,
            bool assignIdOnInsert,
            bool checkElementNames,
            Type documentType,
            IEnumerable documents,
            InsertFlags flags)
            : base(databaseName, collectionName, readerSettings, writerSettings, writeConcern)
        {
            _assignIdOnInsert = assignIdOnInsert;
            _checkElementNames = checkElementNames;
            _documentType = documentType;
            _documents = documents;
            _flags = flags;
        }

        public IEnumerable<WriteConcernResult> Execute(MongoConnection connection)
        {
            List<WriteConcernResult> results = (WriteConcern.Enabled) ? new List<WriteConcernResult>() : null;

            using (var bsonBuffer = new BsonBuffer(new MultiChunkBuffer(BsonChunkPool.Default), true))
            {
                var writerSettings = GetNodeAdjustedWriterSettings(connection.ServerInstance);
                var message = new MongoInsertMessage(writerSettings, CollectionFullName, _checkElementNames, _flags);
                message.WriteToBuffer(bsonBuffer); // must be called before AddDocument

                foreach (var document in _documents)
                {
                    if (document == null)
                    {
                        throw new ArgumentException("Batch contains one or more null documents.");
                    }

                    if (_assignIdOnInsert)
                    {
                        var serializer = BsonSerializer.LookupSerializer(document.GetType());
                        var idProvider = serializer as IBsonIdProvider;
                        if (idProvider != null)
                        {
                            object id;
                            Type idNominalType;
                            IIdGenerator idGenerator;
                            if (idProvider.GetDocumentId(document, out id, out idNominalType, out idGenerator))
                            {
                                if (idGenerator != null && idGenerator.IsEmpty(id))
                                {
                                    id = idGenerator.GenerateId(this, document);
                                    idProvider.SetDocumentId(document, id);
                                }
                            }
                        }
                    }
                    message.AddDocument(bsonBuffer, _documentType, document);

                    if (message.MessageLength > connection.ServerInstance.MaxMessageLength)
                    {
                        byte[] lastDocument = message.RemoveLastDocument(bsonBuffer);

                        if (WriteConcern.Enabled)
                        {
                            WriteGetLastErrorMessage(bsonBuffer, WriteConcern, writerSettings);
                            connection.SendMessage(message.RequestId, bsonBuffer);
                            results.Add(ReadWriteConcernResult(connection));
                        }
                        else if ((_flags & InsertFlags.ContinueOnError) != 0)
                        {
                            connection.SendMessage(message.RequestId, bsonBuffer);
                        }
                        else
                        {
                            // if WriteConcern is disabled and ContinueOnError is false we have to check for errors and stop if sub-batch has error
                            try
                            {
                                WriteGetLastErrorMessage(bsonBuffer, WriteConcern.Acknowledged, writerSettings);
                                connection.SendMessage(message.RequestId, bsonBuffer);
                                ReadWriteConcernResult(connection); // if there is an exception we will catch it and return null
                            }
                            catch (WriteConcernException)
                            {
                                return null;
                            }
                        }

                        message.ResetBatch(bsonBuffer, lastDocument);
                    }
                }

                if (WriteConcern.Enabled)
                {
                    WriteGetLastErrorMessage(bsonBuffer, WriteConcern, writerSettings);
                    connection.SendMessage(message.RequestId, bsonBuffer);
                    results.Add(ReadWriteConcernResult(connection));
                }
                else
                {
                    connection.SendMessage(message.RequestId, bsonBuffer);
                }

                return results;
            }
        }
    }
}
