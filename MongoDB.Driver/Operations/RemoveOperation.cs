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
            WriteConcern writeConcern,
            BsonBinaryWriterSettings writerSettings,
            IMongoQuery query,
            RemoveFlags flags
            )
            : base(databaseName, collectionName, writeConcern, writerSettings)
        {
            _query = query;
            _flags = flags;
        }

        public WriteConcernResult Execute(MongoConnection connection)
        {
            var message = new MongoDeleteMessage(WriterSettings, CollectionFullName, _flags, _query);
            return connection.SendMessage(message, WriteConcern, DatabaseName);
        }
    }
}
