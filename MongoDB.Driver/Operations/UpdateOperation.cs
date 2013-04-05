using MongoDB.Bson.IO;
using MongoDB.Driver.Internal;

namespace MongoDB.Driver.Operations
{
    internal class UpdateOperation : WriteOperation
    {
        private readonly bool _checkElementNames;
        private readonly UpdateFlags _flags;
        private readonly IMongoQuery _query;
        private readonly IMongoUpdate _update;

        public UpdateOperation(
            string databaseName,
            string collectionName,
            WriteConcern writeConcern,
            BsonBinaryWriterSettings writerSettings,
            IMongoQuery query,
            IMongoUpdate update,
            UpdateFlags flags,
            bool checkElementNames)
            : base(databaseName, collectionName, writeConcern, writerSettings)
        {
            _query = query;
            _update = update;
            _flags = flags;
            _checkElementNames = checkElementNames;
        }

        public WriteConcernResult Execute(MongoConnection connection)
        {
            var message = new MongoUpdateMessage(WriterSettings, CollectionFullName, _checkElementNames, _flags, _query, _update);
            return connection.SendMessage(message, WriteConcern, DatabaseName);
        }
    }
}
