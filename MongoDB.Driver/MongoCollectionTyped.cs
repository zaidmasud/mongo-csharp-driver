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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a collection accessed using documents of a Type provided at runtime.
    /// </summary>
    public class MongoCollectionTyped : MongoCollection
    {
        // private fields
        private readonly Type _documentType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the MongoCollectionTyped class.
        /// </summary>
        /// <param name="documentType">The nominal type of the documents contained in this collection.</param>
        /// <param name="database">The database that contains this collection.</param>
        /// <param name="name">The name of the collection.</param>
        /// <param name="settings">The settings to use to access this collection.</param>
        public MongoCollectionTyped(Type documentType, MongoDatabase database, string name, MongoCollectionSettings settings)
            : base(database, name, settings)
        {
            _documentType = documentType;
        }

        // public properties
        /// <summary>
        /// Gets the document type;
        /// </summary>
        public Type DocumentType
        {
            get { return _documentType; }
        }

        // public methods
        /// <summary>
        /// Returns a cursor that can be used to find all documents in this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <returns>A <see cref="MongoCursor"/>.</returns>
        public virtual MongoCursor Find(IMongoQuery query)
        {
            return MongoCursor.Create(_documentType, this, query, Settings.ReadPreference);
        }

        /// <summary>
        /// Returns a cursor that can be used to find all documents in this collection.
        /// </summary>
        /// <returns>A <see cref="MongoCursor"/>.</returns>
        public virtual MongoCursor FindAll()
        {
            return Find(Query.Null);
        }

        /// <summary>
        /// Returns a cursor that can be used to find one document in this collection.
        /// </summary>
        /// <returns>An object (or null if not found).</returns>
        public virtual object FindOne()
        {
            return FindOne(Query.Null);
        }

        /// <summary>
        /// Returns a cursor that can be used to find one document in this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <returns>An object (or null if not found).</returns>
        public virtual object FindOne(IMongoQuery query)
        {
            return Find(query).SetLimit(1).OfType<object>().FirstOrDefault();
        }

        /// <summary>
        /// Returns a cursor that can be used to find one document in this collection by its Id value.
        /// </summary>
        /// <param name="idMemberName">The name of the Id member.</param>
        /// <param name="id">The id of the document.</param>
        /// <returns>An object (or null if not found).</returns>
        public virtual object FindOneById(string idMemberName, object id)
        {
            var serializer = BsonSerializer.LookupSerializer(_documentType);
            var documentSerializer = serializer as IBsonDocumentSerializer;
            if (documentSerializer == null)
            {
                var message = string.Format("The serializer for type {0} does not provide serialization information about the type members (which is needed to serialize the Id).", _documentType.FullName);
                throw new NotSupportedException(message);
            }
            var idSerializationInfo = documentSerializer.GetMemberSerializationInfo(idMemberName);
            var serializedId = idSerializationInfo.SerializeValue(id);
            return FindOne(Query.EQ("_id", serializedId));
        }

        /// <summary>
        /// Runs a geoHaystack search command on this collection.
        /// </summary>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="options">The options for the geoHaystack search (null if none).</param>
        /// <returns>A <see cref="GeoNearResult{BsonDocument}"/>.</returns>
        public virtual GeoHaystackSearchResult GeoHaystackSearch(
            double x,
            double y,
            IMongoGeoHaystackSearchOptions options)
        {
            return GeoHaystackSearchHelper(_documentType, x, y, options);
        }

        /// <summary>
        /// Runs a GeoNear command on this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="limit">The maximum number of results returned.</param>
        /// <returns>A <see cref="GeoNearResult{BsonDocument}"/>.</returns>
        public virtual GeoNearResult GeoNear(
            IMongoQuery query,
            double x,
            double y,
            int limit)
        {
            return GeoNear(query, x, y, limit, GeoNearOptions.Null);
        }

        /// <summary>
        /// Runs a GeoNear command on this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="limit">The maximum number of results returned.</param>
        /// <param name="options">The GeoNear command options (usually a GeoNearOptionsDocument or constructed using the GeoNearOptions builder).</param>
        /// <returns>A <see cref="GeoNearResult{BsonDocument}"/>.</returns>
        public virtual GeoNearResult GeoNear(
            IMongoQuery query,
            double x,
            double y,
            int limit,
            IMongoGeoNearOptions options)
        {
            return GeoNearHelper(_documentType, query, x, y, limit, options);
        }

        /// <summary>
        /// Inserts a document into this collection (see also InsertBatch to insert multiple documents at once).
        /// </summary>
        /// <param name="document">The document to insert.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Insert(object document)
        {
            var options = new MongoInsertOptions();
            return Insert(document, options);
        }

        /// <summary>
        /// Inserts a document into this collection (see also InsertBatch to insert multiple documents at once).
        /// </summary>
        /// <param name="document">The document to insert.</param>
        /// <param name="options">The options to use for this Insert.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Insert(object document, MongoInsertOptions options)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            var results = InsertBatch(new object[] { document }, options);
            return (results == null) ? null : results.Single();
        }

        /// <summary>
        /// Inserts a document into this collection (see also InsertBatch to insert multiple documents at once).
        /// </summary>
        /// <param name="document">The document to insert.</param>
        /// <param name="safeMode">The SafeMode to use for this Insert.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Insert(object document, SafeMode safeMode)
        {
            var options = new MongoInsertOptions { SafeMode = safeMode };
            return Insert(document, options);
        }

        /// <summary>
        /// Inserts multiple documents at once into this collection (see also Insert to insert a single document).
        /// </summary>
        /// <param name="documents">The documents to insert.</param>
        /// <returns>A list of SafeModeResults (or null if SafeMode is not being used).</returns>
        public virtual IEnumerable<SafeModeResult> InsertBatch(IEnumerable documents)
        {
            var options = new MongoInsertOptions();
            return InsertBatch(documents, options);
        }

        /// <summary>
        /// Inserts multiple documents at once into this collection (see also Insert to insert a single document).
        /// </summary>
        /// <param name="documents">The documents to insert.</param>
        /// <param name="options">The options to use for this Insert.</param>
        /// <returns>A list of SafeModeResults (or null if SafeMode is not being used).</returns>
        public virtual IEnumerable<SafeModeResult> InsertBatch(
            IEnumerable documents,
            MongoInsertOptions options)
        {
            if (documents == null)
            {
                throw new ArgumentNullException("documents");
            }
            return InsertBatchHelper(_documentType, documents, options);
        }

        /// <summary>
        /// Inserts multiple documents at once into this collection (see also Insert to insert a single document).
        /// </summary>
        /// <param name="documents">The documents to insert.</param>
        /// <param name="safeMode">The SafeMode to use for this Insert.</param>
        /// <returns>A list of SafeModeResults (or null if SafeMode is not being used).</returns>
        public virtual IEnumerable<SafeModeResult> InsertBatch(
            IEnumerable documents,
            SafeMode safeMode)
        {
            if (safeMode == null)
            {
                throw new ArgumentNullException("safeMode");
            }
            var options = new MongoInsertOptions() { SafeMode = safeMode };
            return InsertBatch(documents, options);
        }

        /// <summary>
        /// Saves a document to this collection. The document must have an identifiable Id field. Based on the value
        /// of the Id field Save will perform either an Insert or an Update.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Save(object document)
        {
            var options = new MongoInsertOptions();
            return Save(document, options);
        }

        /// <summary>
        /// Saves a document to this collection. The document must have an identifiable Id field. Based on the value
        /// of the Id field Save will perform either an Insert or an Update.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="options">The options to use for this Save.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Save(object document, MongoInsertOptions options)
        {
            return SaveHelper(_documentType, document, options);
        }

        /// <summary>
        /// Saves a document to this collection. The document must have an identifiable Id field. Based on the value
        /// of the Id field Save will perform either an Insert or an Update.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="safeMode">The SafeMode to use for this operation.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Save(object document, SafeMode safeMode)
        {
            if (safeMode == null)
            {
                throw new ArgumentNullException("safeMode");
            }
            var options = new MongoInsertOptions { SafeMode = safeMode };
            return Save(document, options);
        }
    }
}
