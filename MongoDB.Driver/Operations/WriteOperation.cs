using MongoDB.Bson.IO;

namespace MongoDB.Driver.Operations
{
    internal abstract class WriteOperation
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly WriteConcern _writeConcern;
        private readonly BsonBinaryWriterSettings _writerSettings;

        protected WriteOperation(
            string databaseName,
            string collectionName,
            WriteConcern writeConcern,
            BsonBinaryWriterSettings writerSettings)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
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
    }
}
