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

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Internal;

namespace MongoDB.Driver.Operations
{
    internal abstract class WriteOperation
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly BsonBinaryReaderSettings _readerSettings;
        private readonly WriteConcern _writeConcern;
        private readonly BsonBinaryWriterSettings _writerSettings;
        private CommandDocument _getLastErrorCommand;

        protected WriteOperation(
            string databaseName,
            string collectionName,
            BsonBinaryReaderSettings readerSettings,
            WriteConcern writeConcern,
            BsonBinaryWriterSettings writerSettings)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _readerSettings = readerSettings;
            _writeConcern = writeConcern;
            _writerSettings = writerSettings;
        }

        protected string CollectionFullName
        {
            get { return _databaseName + "." + _collectionName; }
        }

        protected string CollectionName
        {
            get { return _collectionName; }
        }

        protected string DatabaseName
        {
            get { return _databaseName; }
        }

        protected WriteConcern WriteConcern
        {
            get { return _writeConcern; }
        }

        protected BsonBinaryWriterSettings WriterSettings
        {
            get { return _writerSettings; }
        }

        protected WriteConcernResult ReadWriteConcernResult(MongoConnection connection)
        {
            var writeConcernResultSerializer = BsonSerializer.LookupSerializer(typeof(WriteConcernResult));
            var replyMessage = connection.ReceiveMessage<WriteConcernResult>(_readerSettings, writeConcernResultSerializer, null);

            var writeConcernResult = replyMessage.Documents[0];
            writeConcernResult.Command = _getLastErrorCommand;

            if (!writeConcernResult.Ok)
            {
                var errorMessage = string.Format(
                    "WriteConcern detected an error '{0}'. (response was {1}).",
                    writeConcernResult.ErrorMessage, writeConcernResult.Response.ToJson());
                throw new WriteConcernException(errorMessage, writeConcernResult);
            }
            if (writeConcernResult.HasLastErrorMessage)
            {
                var errorMessage = string.Format(
                    "WriteConcern detected an error '{0}'. (Response was {1}).",
                    writeConcernResult.LastErrorMessage, writeConcernResult.Response.ToJson());
                throw new WriteConcernException(errorMessage, writeConcernResult);
            }

            return writeConcernResult;
        }

        protected void WriteGetLastErrorMessage(BsonBuffer buffer, WriteConcern writeConcern)
        {
            var fsync = (writeConcern.FSync == null) ? null : (BsonValue)writeConcern.FSync;
            var journal = (writeConcern.Journal == null) ? null : (BsonValue)writeConcern.Journal;
            var w = (writeConcern.W == null) ? null : writeConcern.W.ToGetLastErrorWValue();
            var wTimeout = (writeConcern.WTimeout == null) ? null : (BsonValue)(int)writeConcern.WTimeout.Value.TotalMilliseconds;

            _getLastErrorCommand = new CommandDocument
                {
                    { "getlasterror", 1 }, // use all lowercase for backward compatibility
                    { "fsync", fsync, fsync != null },
                    { "j", journal, journal != null },
                    { "w", w, w != null },
                    { "wtimeout", wTimeout, wTimeout != null }
                };

            // piggy back on network transmission for message
            var getLastErrorMessage = new MongoQueryMessage(_writerSettings, DatabaseName + ".$cmd", QueryFlags.None, 0, 1, _getLastErrorCommand, null);
            getLastErrorMessage.WriteToBuffer(buffer);
        }
    }
}
