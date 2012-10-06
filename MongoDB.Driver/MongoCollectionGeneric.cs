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
using System.Linq.Expressions;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a collection accessed using POCOs of type {TDocument} (or a subclass of it).
    /// </summary>
    /// <typeparam name="TDocument">The nominal type of the documents.</typeparam>
    public class MongoCollection<TDocument> : MongoCollection
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the MongoCollection{TDocument} class.
        /// </summary>
        /// <param name="database">The database that contains this collection.</param>
        /// <param name="name">The name of the collection.</param>
        /// <param name="settings">The settings to use to access this collection.</param>
        public MongoCollection(MongoDatabase database, string name, MongoCollectionSettings settings)
            : base(database, name, settings)
        {
        }

        // public methods
        /// <summary>
        /// Returns a cursor that can be used to find all documents in this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <returns>A <see cref="MongoCursor{TDocument}"/>.</returns>
        public virtual MongoCursor<TDocument> Find(IMongoQuery query)
        {
            return new MongoCursor<TDocument>(this, query, Settings.ReadPreference);
        }

        /// <summary>
        /// Returns a cursor that can be used to find all documents in this collection.
        /// </summary>
        /// <returns>A <see cref="MongoCursor{TDocument}"/>.</returns>
        public virtual MongoCursor<TDocument> FindAll()
        {
            return Find(Query.Null);
        }

        /// <summary>
        /// Returns a cursor that can be used to find one document in this collection.
        /// </summary>
        /// <returns>A TDocument (or null if not found).</returns>
        public virtual TDocument FindOne()
        {
            return FindOne(Query.Null);
        }

        /// <summary>
        /// Returns a cursor that can be used to find one document in this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <returns>A TDocument (or null if not found).</returns>
        public virtual TDocument FindOne(IMongoQuery query)
        {
            return Find(query).SetLimit(1).FirstOrDefault();
        }

        /// <summary>
        /// Returns a cursor that can be used to find one document in this collection by its Id value.
        /// </summary>
        /// <typeparam name="TId">The nominal type of the Id.</typeparam>
        /// <param name="idMemberExpression">The member expression representing the Id of the document.</param>
        /// <param name="id">The id of the document.</param>
        /// <returns>A TDocument (or null if not found).</returns>
        public virtual TDocument FindOneById<TId>(Expression<Func<TDocument, TId>> idMemberExpression, TId id)
        {
            return FindOne(Query<TDocument>.EQ(idMemberExpression, id));
        }

        /// <summary>
        /// Runs a geoHaystack search command on this collection.
        /// </summary>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="options">The options for the geoHaystack search (null if none).</param>
        /// <returns>A <see cref="GeoNearResult{TDocument}"/>.</returns>
        public virtual GeoHaystackSearchResult<TDocument> GeoHaystackSearch(
            double x,
            double y,
            IMongoGeoHaystackSearchOptions options)
        {
            return GeoHaystackSearchHelper<TDocument>(x, y, options);
        }

        /// <summary>
        /// Runs a GeoNear command on this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="limit">The maximum number of results returned.</param>
        /// <returns>A <see cref="GeoNearResult{TDocument}"/>.</returns>
        public virtual GeoNearResult<TDocument> GeoNear(
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
        /// <returns>A <see cref="GeoNearResult{TDocument}"/>.</returns>
        public virtual GeoNearResult<TDocument> GeoNear(
            IMongoQuery query,
            double x,
            double y,
            int limit,
            IMongoGeoNearOptions options)
        {
            return GeoNearHelper<TDocument>(query, x, y, limit, options);
        }

        /// <summary>
        /// Inserts a document into this collection (see also InsertBatch to insert multiple documents at once).
        /// </summary>
        /// <param name="document">The document to insert.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Insert(TDocument document)
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
        public virtual SafeModeResult Insert(TDocument document, MongoInsertOptions options)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            var results = InsertBatch(new TDocument[] { document }, options);
            return (results == null) ? null : results.Single();
        }

        /// <summary>
        /// Inserts a document into this collection (see also InsertBatch to insert multiple documents at once).
        /// </summary>
        /// <param name="document">The document to insert.</param>
        /// <param name="safeMode">The SafeMode to use for this Insert.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Insert(TDocument document, SafeMode safeMode)
        {
            var options = new MongoInsertOptions { SafeMode = safeMode };
            return Insert(document, options);
        }

        /// <summary>
        /// Inserts multiple documents at once into this collection (see also Insert to insert a single document).
        /// </summary>
        /// <param name="documents">The documents to insert.</param>
        /// <returns>A list of SafeModeResults (or null if SafeMode is not being used).</returns>
        public virtual IEnumerable<SafeModeResult> InsertBatch(IEnumerable<TDocument> documents)
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
            IEnumerable<TDocument> documents,
            MongoInsertOptions options)
        {
            if (documents == null)
            {
                throw new ArgumentNullException("documents");
            }
            return InsertBatchHelper(typeof(TDocument), documents.Cast<object>(), options);
        }

        /// <summary>
        /// Inserts multiple documents at once into this collection (see also Insert to insert a single document).
        /// </summary>
        /// <param name="documents">The documents to insert.</param>
        /// <param name="safeMode">The SafeMode to use for this Insert.</param>
        /// <returns>A list of SafeModeResults (or null if SafeMode is not being used).</returns>
        public virtual IEnumerable<SafeModeResult> InsertBatch(
            IEnumerable<TDocument> documents,
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
        public virtual SafeModeResult Save(TDocument document)
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
        public virtual SafeModeResult Save(TDocument document, MongoInsertOptions options)
        {
            return SaveHelper(typeof(TDocument), document, options);
        }

        /// <summary>
        /// Saves a document to this collection. The document must have an identifiable Id field. Based on the value
        /// of the Id field Save will perform either an Insert or an Update.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="safeMode">The SafeMode to use for this operation.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Save(TDocument document, SafeMode safeMode)
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
