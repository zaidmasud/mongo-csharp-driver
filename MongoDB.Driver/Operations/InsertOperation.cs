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
            WriteConcern writeConcern,
            BsonBinaryWriterSettings writerSettings,
            bool assignIdOnInsert,
            bool checkElementNames,
            Type documentType,
            IEnumerable documents,
            InsertFlags flags
            )
            : base(databaseName, collectionName, writeConcern, writerSettings)
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
                var message = new MongoInsertMessage(WriterSettings, CollectionFullName, _checkElementNames, _flags);
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

                        if (WriteConcern.Enabled || (_flags & InsertFlags.ContinueOnError) != 0)
                        {
                            var intermediateResult = connection.SendMessage(bsonBuffer, message, WriteConcern, DatabaseName);
                            if (WriteConcern.Enabled) { results.Add(intermediateResult); }
                        }
                        else
                        {
                            // if WriteConcern is disabled and ContinueOnError is false we have to check for errors and stop if sub-batch has error
                            try
                            {
                                connection.SendMessage(bsonBuffer, message, WriteConcern.Acknowledged, DatabaseName);
                            }
                            catch (WriteConcernException)
                            {
                                return null;
                            }
                        }

                        message.ResetBatch(bsonBuffer, lastDocument);
                    }
                }

                var finalResult = connection.SendMessage(bsonBuffer, message, WriteConcern, DatabaseName);
                if (WriteConcern.Enabled) { results.Add(finalResult); }

                return results;
            }
        }
    }
}
