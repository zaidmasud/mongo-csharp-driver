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
    /// Represents a base class for MongoCollection implementations.
    /// </summary>
    public abstract class MongoCollection
    {
        // private fields
        private MongoServer _server;
        private MongoDatabase _database;
        private MongoCollectionSettings _settings;
        private string _name;

        // constructors
        /// <summary>
        /// Protected constructor for abstract base class.
        /// </summary>
        /// <param name="database">The database that contains this collection.</param>
        /// <param name="name">The name of the collection.</param>
        /// <param name="settings">The settings to use to access this collection.</param>
        protected MongoCollection(MongoDatabase database, string name, MongoCollectionSettings settings)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            string message;
            if (!database.IsCollectionNameValid(name, out message))
            {
                throw new ArgumentOutOfRangeException("name", message);
            }

            settings = settings.Clone();
            settings.ApplyInheritedSettings(database.Settings);
            settings.Freeze();

            _server = database.Server;
            _database = database;
            _settings = settings;
            _name = name;
        }

        // public properties
        /// <summary>
        /// Gets the database that contains this collection.
        /// </summary>
        public virtual MongoDatabase Database
        {
            get { return _database; }
        }

        /// <summary>
        /// Gets the fully qualified name of this collection.
        /// </summary>
        public virtual string FullName
        {
            get { return _database.Name + "." + _name; }
        }

        /// <summary>
        /// Gets the name of this collection.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the settings being used to access this collection.
        /// </summary>
        public virtual MongoCollectionSettings Settings
        {
            get { return _settings; }
        }

        // public methods
        /// <summary>
        /// Runs an aggregation framework command.
        /// </summary>
        /// <param name="operations">The pipeline operations.</param>
        /// <returns>An AggregateResult.</returns>
        public virtual AggregateResult Aggregate(IEnumerable<BsonDocument> operations)
        {
            var pipeline = new BsonArray();
            foreach (var operation in operations)
            {
                pipeline.Add(operation);
            }

            var aggregateCommand = new CommandDocument
            {
                { "aggregate", _name },
                { "pipeline", pipeline }
            };
            return RunCommand<AggregateResult>(aggregateCommand);
        }

        /// <summary>
        /// Runs an aggregation framework command.
        /// </summary>
        /// <param name="operations">The pipeline operations.</param>
        /// <returns>An AggregateResult.</returns>
        public virtual AggregateResult Aggregate(params BsonDocument[] operations)
        {
            return Aggregate((IEnumerable<BsonDocument>) operations);
        }

        /// <summary>
        /// Counts the number of documents in this collection.
        /// </summary>
        /// <returns>The number of documents in this collection.</returns>
        public virtual long Count()
        {
            return Count(Query.Null);
        }

        /// <summary>
        /// Counts the number of documents in this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <returns>The number of documents in this collection that match the query.</returns>
        public virtual long Count(IMongoQuery query)
        {
            var command = new CommandDocument
            {
                { "count", _name },
                { "query", BsonDocumentWrapper.Create(query), query != null } // query is optional
            };
            var result = RunCommand(command);
            return result.Response["n"].ToInt64();
        }

        /// <summary>
        /// Creates an index for this collection.
        /// </summary>
        /// <param name="keys">The indexed fields (usually an IndexKeysDocument or constructed using the IndexKeys builder).</param>
        /// <param name="options">The index options(usually an IndexOptionsDocument or created using the IndexOption builder).</param>
        /// <returns>A SafeModeResult.</returns>
        public virtual SafeModeResult CreateIndex(IMongoIndexKeys keys, IMongoIndexOptions options)
        {
            var keysDocument = keys.ToBsonDocument();
            var optionsDocument = options.ToBsonDocument();
            var indexes = _database.GetCollection("system.indexes");
            var indexName = GetIndexName(keysDocument, optionsDocument);
            var index = new BsonDocument
            {
                { "name", indexName },
                { "ns", FullName },
                { "key", keysDocument }
            };
            if (optionsDocument != null)
            {
                index.Merge(optionsDocument);
            }
            var insertOptions = new MongoInsertOptions
            {
                CheckElementNames = false,
                SafeMode = SafeMode.True
            };
            var result = indexes.Insert(index, insertOptions);
            return result;
        }

        /// <summary>
        /// Creates an index for this collection.
        /// </summary>
        /// <param name="keys">The indexed fields (usually an IndexKeysDocument or constructed using the IndexKeys builder).</param>
        /// <returns>A SafeModeResult.</returns>
        public virtual SafeModeResult CreateIndex(IMongoIndexKeys keys)
        {
            return CreateIndex(keys, IndexOptions.Null);
        }

        /// <summary>
        /// Creates an index for this collection.
        /// </summary>
        /// <param name="keyNames">The names of the indexed fields.</param>
        /// <returns>A SafeModeResult.</returns>
        public virtual SafeModeResult CreateIndex(params string[] keyNames)
        {
            return CreateIndex(IndexKeys.Ascending(keyNames));
        }

        /// <summary>
        /// Returns the distinct values for a given field.
        /// </summary>
        /// <param name="key">The key of the field.</param>
        /// <returns>The distint values of the field.</returns>
        public virtual IEnumerable<BsonValue> Distinct(string key)
        {
            return Distinct(key, Query.Null);
        }

        /// <summary>
        /// Returns the distinct values for a given field for documents that match a query.
        /// </summary>
        /// <param name="key">The key of the field.</param>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <returns>The distint values of the field.</returns>
        public virtual IEnumerable<BsonValue> Distinct(string key, IMongoQuery query)
        {
            var command = new CommandDocument
            {
                { "distinct", _name },
                { "key", key },
                { "query", BsonDocumentWrapper.Create(query), query != null } // query is optional
            };
            var result = RunCommand(command);
            return result.Response["values"].AsBsonArray;
        }

        /// <summary>
        /// Drops this collection.
        /// </summary>
        /// <returns>A CommandResult.</returns>
        public virtual CommandResult Drop()
        {
            return _database.DropCollection(_name);
        }

        /// <summary>
        /// Drops all indexes on this collection.
        /// </summary>
        /// <returns>A <see cref="CommandResult"/>.</returns>
        public virtual CommandResult DropAllIndexes()
        {
            return DropIndexByName("*");
        }

        /// <summary>
        /// Drops an index on this collection.
        /// </summary>
        /// <param name="keys">The indexed fields (usually an IndexKeysDocument or constructed using the IndexKeys builder).</param>
        /// <returns>A <see cref="CommandResult"/>.</returns>
        public virtual CommandResult DropIndex(IMongoIndexKeys keys)
        {
            string indexName = GetIndexName(keys.ToBsonDocument(), null);
            return DropIndexByName(indexName);
        }

        /// <summary>
        /// Drops an index on this collection.
        /// </summary>
        /// <param name="keyNames">The names of the indexed fields.</param>
        /// <returns>A <see cref="CommandResult"/>.</returns>
        public virtual CommandResult DropIndex(params string[] keyNames)
        {
            string indexName = GetIndexName(keyNames);
            return DropIndexByName(indexName);
        }

        /// <summary>
        /// Drops an index on this collection.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>A <see cref="CommandResult"/>.</returns>
        public virtual CommandResult DropIndexByName(string indexName)
        {
            // remove from cache first (even if command ends up failing)
            if (indexName == "*")
            {
                _server.IndexCache.Reset(this);
            }
            else
            {
                _server.IndexCache.Remove(this, indexName);
            }
            var command = new CommandDocument
            {
                { "deleteIndexes", _name }, // not FullName
                { "index", indexName }
            };
            try
            {
                return RunCommand(command);
            }
            catch (MongoCommandException ex)
            {
                if (ex.CommandResult.ErrorMessage == "ns not found")
                {
                    return ex.CommandResult;
                }
                throw;
            }
        }

        /// <summary>
        /// Ensures that the desired index exists and creates it if it does not.
        /// </summary>
        /// <param name="keys">The indexed fields (usually an IndexKeysDocument or constructed using the IndexKeys builder).</param>
        /// <param name="options">The index options(usually an IndexOptionsDocument or created using the IndexOption builder).</param>
        public virtual void EnsureIndex(IMongoIndexKeys keys, IMongoIndexOptions options)
        {
            var keysDocument = keys.ToBsonDocument();
            var optionsDocument = options.ToBsonDocument();
            var indexName = GetIndexName(keysDocument, optionsDocument);
            if (!_server.IndexCache.Contains(this, indexName))
            {
                CreateIndex(keys, options);
                _server.IndexCache.Add(this, indexName);
            }
        }

        /// <summary>
        /// Ensures that the desired index exists and creates it if it does not.
        /// </summary>
        /// <param name="keys">The indexed fields (usually an IndexKeysDocument or constructed using the IndexKeys builder).</param>
        public virtual void EnsureIndex(IMongoIndexKeys keys)
        {
            EnsureIndex(keys, IndexOptions.Null);
        }

        /// <summary>
        /// Ensures that the desired index exists and creates it if it does not.
        /// </summary>
        /// <param name="keyNames">The names of the indexed fields.</param>
        public virtual void EnsureIndex(params string[] keyNames)
        {
            string indexName = GetIndexName(keyNames);
            if (!_server.IndexCache.Contains(this, indexName))
            {
                CreateIndex(IndexKeys.Ascending(keyNames), IndexOptions.SetName(indexName));
                _server.IndexCache.Add(this, indexName);
            }
        }

        /// <summary>
        /// Tests whether this collection exists.
        /// </summary>
        /// <returns>True if this collection exists.</returns>
        public virtual bool Exists()
        {
            return _database.CollectionExists(_name);
        }

        /// <summary>
        /// Finds one matching document using the query and sortBy parameters and applies the specified update to it.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="sortBy">The sort order to select one of the matching documents.</param>
        /// <param name="update">The update to apply to the matching document.</param>
        /// <returns>A <see cref="FindAndModifyResult"/>.</returns>
        public virtual FindAndModifyResult FindAndModify(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update)
        {
            return FindAndModify(query, sortBy, update, false);
        }

        /// <summary>
        /// Finds one matching document using the query and sortBy parameters and applies the specified update to it.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="sortBy">The sort order to select one of the matching documents.</param>
        /// <param name="update">The update to apply to the matching document.</param>
        /// <param name="returnNew">Whether to return the new or old version of the modified document in the <see cref="FindAndModifyResult"/>.</param>
        /// <returns>A <see cref="FindAndModifyResult"/>.</returns>
        public virtual FindAndModifyResult FindAndModify(
            IMongoQuery query,
            IMongoSortBy sortBy,
            IMongoUpdate update,
            bool returnNew)
        {
            return FindAndModify(query, sortBy, update, returnNew, false);
        }

        /// <summary>
        /// Finds one matching document using the query and sortBy parameters and applies the specified update to it.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="sortBy">The sort order to select one of the matching documents.</param>
        /// <param name="update">The update to apply to the matching document.</param>
        /// <param name="returnNew">Whether to return the new or old version of the modified document in the <see cref="FindAndModifyResult"/>.</param>
        /// <param name="upsert">Whether to do an upsert if no matching document is found.</param>
        /// <returns>A <see cref="FindAndModifyResult"/>.</returns>
        public virtual FindAndModifyResult FindAndModify(
            IMongoQuery query,
            IMongoSortBy sortBy,
            IMongoUpdate update,
            bool returnNew,
            bool upsert)
        {
            return FindAndModify(query, sortBy, update, Fields.Null, returnNew, upsert);
        }

        /// <summary>
        /// Finds one matching document using the query and sortBy parameters and applies the specified update to it.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="sortBy">The sort order to select one of the matching documents.</param>
        /// <param name="update">The update to apply to the matching document.</param>
        /// <param name="fields">Which fields of the modified document to return in the <see cref="FindAndModifyResult"/>.</param>
        /// <param name="returnNew">Whether to return the new or old version of the modified document in the <see cref="FindAndModifyResult"/>.</param>
        /// <param name="upsert">Whether to do an upsert if no matching document is found.</param>
        /// <returns>A <see cref="FindAndModifyResult"/>.</returns>
        public virtual FindAndModifyResult FindAndModify(
            IMongoQuery query,
            IMongoSortBy sortBy,
            IMongoUpdate update,
            IMongoFields fields,
            bool returnNew,
            bool upsert)
        {
            var command = new CommandDocument
            {
                { "findAndModify", _name },
                { "query", BsonDocumentWrapper.Create(query), query != null }, // query is optional
                { "sort", BsonDocumentWrapper.Create(sortBy), sortBy != null }, // sortBy is optional
                { "update", BsonDocumentWrapper.Create(update, true) }, // isUpdateDocument = true
                { "fields", BsonDocumentWrapper.Create(fields), fields != null }, // fields is optional
                { "new", true, returnNew },
                { "upsert", true, upsert}
            };
            try
            {
                return RunCommand<FindAndModifyResult>(command);
            }
            catch (MongoCommandException ex)
            {
                if (ex.CommandResult.ErrorMessage == "No matching object found")
                {
                    // create a new command result with what the server should have responded
                    var response = new BsonDocument
                    {
                        { "value", BsonNull.Value },
                        { "ok", true }
                    };
                    var result = new FindAndModifyResult();
                    result.Initialize(command, response);
                    return result;
                }
                throw;
            }
        }

        /// <summary>
        /// Finds one matching document using the query and sortBy parameters and removes it from this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="sortBy">The sort order to select one of the matching documents.</param>
        /// <returns>A <see cref="FindAndModifyResult"/>.</returns>
        public virtual FindAndModifyResult FindAndRemove(IMongoQuery query, IMongoSortBy sortBy)
        {
            var command = new CommandDocument
            {
                { "findAndModify", _name },
                { "query", BsonDocumentWrapper.Create(query), query != null }, // query is optional
                { "sort", BsonDocumentWrapper.Create(sortBy), sortBy != null }, // sort is optional
                { "remove", true }
            };
            try
            {
                return RunCommand<FindAndModifyResult>(command);
            }
            catch (MongoCommandException ex)
            {
                if (ex.CommandResult.ErrorMessage == "No matching object found")
                {
                    // create a new command result with what the server should have responded
                    var response = new BsonDocument
                    {
                        { "value", BsonNull.Value },
                        { "ok", true }
                    };
                    var result = new FindAndModifyResult();
                    result.Initialize(command, response);
                    return result;
                }
                throw;
            }
        }

        /// <summary>
        /// Runs a geoHaystack search command on this collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the found documents.</typeparam>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="options">The options for the geoHaystack search (null if none).</param>
        /// <returns>A <see cref="GeoNearResult{TDocument}"/>.</returns>
        protected GeoHaystackSearchResult<TDocument> GeoHaystackSearchHelper<TDocument>(
            double x,
            double y,
            IMongoGeoHaystackSearchOptions options)
        {
            var command = new CommandDocument
            {
                { "geoSearch", _name },
                { "near", new BsonArray { x, y } }
            };
            command.Merge(options.ToBsonDocument());
            return RunCommand<GeoHaystackSearchResult<TDocument>>(command);
        }

        /// <summary>
        /// Runs a geoHaystack search command on this collection.
        /// </summary>
        /// <param name="documentType">The type to deserialize the documents as.</param>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="options">The options for the geoHaystack search (null if none).</param>
        /// <returns>A <see cref="GeoNearResult{TDocument}"/>.</returns>
        protected GeoHaystackSearchResult GeoHaystackSearchHelper(
            Type documentType,
            double x,
            double y,
            IMongoGeoHaystackSearchOptions options)
        {
            var command = new CommandDocument
            {
                { "geoSearch", _name },
                { "near", new BsonArray { x, y } }
            };
            command.Merge(options.ToBsonDocument());
            var geoHaystackSearchResultDefinition = typeof(GeoHaystackSearchResult<>);
            var geoHaystackSearchResultType = geoHaystackSearchResultDefinition.MakeGenericType(documentType);
            return (GeoHaystackSearchResult)RunCommand(geoHaystackSearchResultType, command);
        }

        /// <summary>
        /// Runs a GeoNear command on this collection.
        /// </summary>
        /// <typeparam name="TDocument">The type to deserialize the documents as.</typeparam>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="limit">The maximum number of results returned.</param>
        /// <param name="options">The GeoNear command options (usually a GeoNearOptionsDocument or constructed using the GeoNearOptions builder).</param>
        /// <returns>A <see cref="GeoNearResult{TDocument}"/>.</returns>
        protected GeoNearResult<TDocument> GeoNearHelper<TDocument>(
            IMongoQuery query,
            double x,
            double y,
            int limit,
            IMongoGeoNearOptions options)
        {
            var command = new CommandDocument
            {
                { "geoNear", _name },
                { "near", new BsonArray { x, y } },
                { "num", limit },
                { "query", BsonDocumentWrapper.Create(query), query != null } // query is optional
            };
            command.Merge(options.ToBsonDocument());
            return RunCommand<GeoNearResult<TDocument>>(command);
        }

        /// <summary>
        /// Runs a GeoNear command on this collection.
        /// </summary>
        /// <param name="documentType">The type to deserialize the documents as.</param>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="x">The x coordinate of the starting location.</param>
        /// <param name="y">The y coordinate of the starting location.</param>
        /// <param name="limit">The maximum number of results returned.</param>
        /// <param name="options">The GeoNear command options (usually a GeoNearOptionsDocument or constructed using the GeoNearOptions builder).</param>
        /// <returns>A <see cref="GeoNearResult{TDocument}"/>.</returns>
        protected GeoNearResult GeoNearHelper(
            Type documentType,
            IMongoQuery query,
            double x,
            double y,
            int limit,
            IMongoGeoNearOptions options)
        {
            var command = new CommandDocument
            {
                { "geoNear", _name },
                { "near", new BsonArray { x, y } },
                { "num", limit },
                { "query", BsonDocumentWrapper.Create(query), query != null } // query is optional
            };
            command.Merge(options.ToBsonDocument());
            var geoNearResultDefinition = typeof(GeoNearResult<>);
            var geoNearResultType = geoNearResultDefinition.MakeGenericType(documentType);
            return (GeoNearResult)RunCommand(geoNearResultType, command);
        }

        /// <summary>
        /// Gets the indexes for this collection.
        /// </summary>
        /// <returns>A list of BsonDocuments that describe the indexes.</returns>
        public virtual GetIndexesResult GetIndexes()
        {
            var indexes = _database.GetCollection("system.indexes");
            var query = Query.EQ("ns", FullName);
            return new GetIndexesResult(indexes.Find(query).ToArray()); // ToArray forces execution of the query
        }

        /// <summary>
        /// Gets the stats for this collection.
        /// </summary>
        /// <returns>The stats for this collection as a <see cref="CollectionStatsResult"/>.</returns>
        public virtual CollectionStatsResult GetStats()
        {
            var command = new CommandDocument("collstats", _name);
            return RunCommand<CollectionStatsResult>(command);
        }

        /// <summary>
        /// Gets the total data size for this collection (data + indexes).
        /// </summary>
        /// <returns>The total data size.</returns>
        public virtual long GetTotalDataSize()
        {
            var totalSize = GetStats().DataSize;
            foreach (var index in GetIndexes())
            {
                var indexCollectionName = string.Format("{0}.${1}", _name, index.Name);
                var indexCollection = _database.GetCollection(indexCollectionName);
                totalSize += indexCollection.GetStats().DataSize;
            }
            return totalSize;
        }

        /// <summary>
        /// Gets the total storage size for this collection (data + indexes + overhead).
        /// </summary>
        /// <returns>The total storage size.</returns>
        public virtual long GetTotalStorageSize()
        {
            var totalSize = GetStats().StorageSize;
            foreach (var index in GetIndexes())
            {
                var indexCollectionName = string.Format("{0}.${1}", _name, index.Name);
                var indexCollection = _database.GetCollection(indexCollectionName);
                totalSize += indexCollection.GetStats().StorageSize;
            }
            return totalSize;
        }

        /// <summary>
        /// Runs the group command on this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="keyFunction">A JavaScript function that returns the key value to group on.</param>
        /// <param name="initial">Initial value passed to the reduce function for each group.</param>
        /// <param name="reduce">A JavaScript function that is called for each matching document in a group.</param>
        /// <param name="finalize">A JavaScript function that is called at the end of the group command.</param>
        /// <returns>A list of results as BsonDocuments.</returns>
        public virtual IEnumerable<BsonDocument> Group(
            IMongoQuery query,
            BsonJavaScript keyFunction,
            BsonDocument initial,
            BsonJavaScript reduce,
            BsonJavaScript finalize)
        {
            if (keyFunction == null)
            {
                throw new ArgumentNullException("keyFunction");
            }
            if (initial == null)
            {
                throw new ArgumentNullException("initial");
            }
            if (reduce == null)
            {
                throw new ArgumentNullException("reduce");
            }

            var command = new CommandDocument
            {
                {
                    "group", new BsonDocument
                    {
                        { "ns", _name },
                        { "condition", BsonDocumentWrapper.Create(query), query != null }, // condition is optional
                        { "$keyf", keyFunction },
                        { "initial", initial },
                        { "$reduce", reduce },
                        { "finalize", finalize, finalize != null } // finalize is optional
                    }
                }
            };
            var result = RunCommand(command);
            return result.Response["retval"].AsBsonArray.Values.Cast<BsonDocument>();
        }

        /// <summary>
        /// Runs the group command on this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="keys">The names of the fields to group on.</param>
        /// <param name="initial">Initial value passed to the reduce function for each group.</param>
        /// <param name="reduce">A JavaScript function that is called for each matching document in a group.</param>
        /// <param name="finalize">A JavaScript function that is called at the end of the group command.</param>
        /// <returns>A list of results as BsonDocuments.</returns>
        public virtual IEnumerable<BsonDocument> Group(
            IMongoQuery query,
            IMongoGroupBy keys,
            BsonDocument initial,
            BsonJavaScript reduce,
            BsonJavaScript finalize)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if (initial == null)
            {
                throw new ArgumentNullException("initial");
            }
            if (reduce == null)
            {
                throw new ArgumentNullException("reduce");
            }

            var command = new CommandDocument
            {
                {
                    "group", new BsonDocument
                    {
                        { "ns", _name },
                        { "condition", BsonDocumentWrapper.Create(query), query != null }, // condition is optional
                        { "key", BsonDocumentWrapper.Create(keys) },
                        { "initial", initial },
                        { "$reduce", reduce },
                        { "finalize", finalize, finalize != null } // finalize is optional
                    }
                }
            };
            var result = RunCommand(command);
            return result.Response["retval"].AsBsonArray.Values.Cast<BsonDocument>();
        }

        /// <summary>
        /// Runs the group command on this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="key">The name of the field to group on.</param>
        /// <param name="initial">Initial value passed to the reduce function for each group.</param>
        /// <param name="reduce">A JavaScript function that is called for each matching document in a group.</param>
        /// <param name="finalize">A JavaScript function that is called at the end of the group command.</param>
        /// <returns>A list of results as BsonDocuments.</returns>
        public virtual IEnumerable<BsonDocument> Group(
            IMongoQuery query,
            string key,
            BsonDocument initial,
            BsonJavaScript reduce,
            BsonJavaScript finalize)
        {
            return Group(query, GroupBy.Keys(key), initial, reduce, finalize);
        }

        /// <summary>
        /// Tests whether an index exists.
        /// </summary>
        /// <param name="keys">The indexed fields (usually an IndexKeysDocument or constructed using the IndexKeys builder).</param>
        /// <returns>True if the index exists.</returns>
        public virtual bool IndexExists(IMongoIndexKeys keys)
        {
            string indexName = GetIndexName(keys.ToBsonDocument(), null);
            return IndexExistsByName(indexName);
        }

        /// <summary>
        /// Tests whether an index exists.
        /// </summary>
        /// <param name="keyNames">The names of the fields in the index.</param>
        /// <returns>True if the index exists.</returns>
        public virtual bool IndexExists(params string[] keyNames)
        {
            string indexName = GetIndexName(keyNames);
            return IndexExistsByName(indexName);
        }

        /// <summary>
        /// Tests whether an index exists.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>True if the index exists.</returns>
        public virtual bool IndexExistsByName(string indexName)
        {
            var indexes = _database.GetCollection("system.indexes");
            var query = Query.And(Query.EQ("name", indexName), Query.EQ("ns", FullName));
            return indexes.Count(query) != 0;
        }

        /// <summary>
        /// Inserts multiple documents at once into this collection (see also Insert to insert a single document).
        /// </summary>
        /// <param name="nominalType">The nominal type of the documents to insert.</param>
        /// <param name="documents">The documents to insert.</param>
        /// <param name="options">The options to use for this Insert.</param>
        /// <returns>A list of SafeModeResults (or null if SafeMode is not being used).</returns>
        protected IEnumerable<SafeModeResult> InsertBatchHelper(
            Type nominalType,
            IEnumerable documents,
            MongoInsertOptions options)
        {
            if (nominalType == null)
            {
                throw new ArgumentNullException("nominalType");
            }
            if (documents == null)
            {
                throw new ArgumentNullException("documents");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var connection = _server.AcquireConnection(_database, ReadPreference.Primary);
            try
            {
                var safeMode = options.SafeMode ?? _settings.SafeMode;

                List<SafeModeResult> results = (safeMode.Enabled) ? new List<SafeModeResult>() : null;

                var writerSettings = GetWriterSettings(connection);
                using (var message = new MongoInsertMessage(writerSettings, FullName, options.CheckElementNames, options.Flags))
                {
                    message.WriteToBuffer(); // must be called before AddDocument

                    foreach (var document in documents)
                    {
                        if (document == null)
                        {
                            throw new ArgumentException("Batch contains one or more null documents.");
                        }

                        if (_settings.AssignIdOnInsert.Value)
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
                        message.AddDocument(nominalType, document);

                        if (message.MessageLength > connection.ServerInstance.MaxMessageLength)
                        {
                            byte[] lastDocument = message.RemoveLastDocument();
                            var intermediateResult = connection.SendMessage(message, safeMode, _database.Name);
                            if (safeMode.Enabled) { results.Add(intermediateResult); }
                            message.ResetBatch(lastDocument);
                        }
                    }

                    var finalResult = connection.SendMessage(message, safeMode, _database.Name);
                    if (safeMode.Enabled) { results.Add(finalResult); }

                    return results;
                }
            }
            finally
            {
                _server.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Tests whether this collection is capped.
        /// </summary>
        /// <returns>True if this collection is capped.</returns>
        public virtual bool IsCapped()
        {
            return GetStats().IsCapped;
        }

        /// <summary>
        /// Runs a Map/Reduce command on this collection.
        /// </summary>
        /// <param name="map">A JavaScript function called for each document.</param>
        /// <param name="reduce">A JavaScript function called on the values emitted by the map function.</param>
        /// <param name="options">Options for this map/reduce command (see <see cref="MapReduceOptionsDocument"/>, <see cref="MapReduceOptionsWrapper"/> and the <see cref="MapReduceOptions"/> builder).</param>
        /// <returns>A <see cref="MapReduceResult"/>.</returns>
        public virtual MapReduceResult MapReduce(
            BsonJavaScript map,
            BsonJavaScript reduce,
            IMongoMapReduceOptions options)
        {
            var command = new CommandDocument
            {
                { "mapreduce", _name },
                { "map", map },
                { "reduce", reduce }
            };
            command.AddRange(options.ToBsonDocument());
            var result = RunCommand<MapReduceResult>(command);
            result.SetInputDatabase(_database);
            return result;
        }

        /// <summary>
        /// Runs a Map/Reduce command on document in this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="map">A JavaScript function called for each document.</param>
        /// <param name="reduce">A JavaScript function called on the values emitted by the map function.</param>
        /// <param name="options">Options for this map/reduce command (see <see cref="MapReduceOptionsDocument"/>, <see cref="MapReduceOptionsWrapper"/> and the <see cref="MapReduceOptions"/> builder).</param>
        /// <returns>A <see cref="MapReduceResult"/>.</returns>
        public virtual MapReduceResult MapReduce(
            IMongoQuery query,
            BsonJavaScript map,
            BsonJavaScript reduce,
            IMongoMapReduceOptions options)
        {
            // create a new set of options because we don't want to modify caller's data
            options = MapReduceOptions.SetQuery(query).AddOptions(options.ToBsonDocument());
            return MapReduce(map, reduce, options);
        }

        /// <summary>
        /// Runs a Map/Reduce command on document in this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="map">A JavaScript function called for each document.</param>
        /// <param name="reduce">A JavaScript function called on the values emitted by the map function.</param>
        /// <returns>A <see cref="MapReduceResult"/>.</returns>
        public virtual MapReduceResult MapReduce(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce)
        {
            var options = MapReduceOptions.SetQuery(query).SetOutput(MapReduceOutput.Inline);
            return MapReduce(map, reduce, options);
        }

        /// <summary>
        /// Runs a Map/Reduce command on this collection.
        /// </summary>
        /// <param name="map">A JavaScript function called for each document.</param>
        /// <param name="reduce">A JavaScript function called on the values emitted by the map function.</param>
        /// <returns>A <see cref="MapReduceResult"/>.</returns>
        public virtual MapReduceResult MapReduce(BsonJavaScript map, BsonJavaScript reduce)
        {
            var options = MapReduceOptions.SetOutput(MapReduceOutput.Inline);
            return MapReduce(map, reduce, options);
        }

        /// <summary>
        /// Runs the ReIndex command on this collection.
        /// </summary>
        /// <returns>A CommandResult.</returns>
        public virtual CommandResult ReIndex()
        {
            var command = new CommandDocument("reIndex", _name);
            return RunCommand(command);
        }

        /// <summary>
        /// Removes documents from this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Remove(IMongoQuery query)
        {
            return Remove(query, RemoveFlags.None, null);
        }

        /// <summary>
        /// Removes documents from this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="safeMode">The SafeMode to use for this operation.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Remove(IMongoQuery query, SafeMode safeMode)
        {
            return Remove(query, RemoveFlags.None, safeMode);
        }

        /// <summary>
        /// Removes documents from this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="flags">The flags for this Remove (see <see cref="RemoveFlags"/>).</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Remove(IMongoQuery query, RemoveFlags flags)
        {
            return Remove(query, flags, null);
        }

        /// <summary>
        /// Removes documents from this collection that match a query.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="flags">The flags for this Remove (see <see cref="RemoveFlags"/>).</param>
        /// <param name="safeMode">The SafeMode to use for this operation.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Remove(IMongoQuery query, RemoveFlags flags, SafeMode safeMode)
        {
            var connection = _server.AcquireConnection(_database, ReadPreference.Primary);
            try
            {
                var writerSettings = GetWriterSettings(connection);
                using (var message = new MongoDeleteMessage(writerSettings, FullName, flags, query))
                {
                    return connection.SendMessage(message, safeMode ?? _settings.SafeMode, _database.Name);
                }
            }
            finally
            {
                _server.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Removes all documents from this collection (see also <see cref="Drop"/>).
        /// </summary>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult RemoveAll()
        {
            return Remove(Query.Null, RemoveFlags.None, null);
        }

        /// <summary>
        /// Removes all documents from this collection (see also <see cref="Drop"/>).
        /// </summary>
        /// <param name="safeMode">The SafeMode to use for this operation.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult RemoveAll(SafeMode safeMode)
        {
            return Remove(Query.Null, RemoveFlags.None, safeMode);
        }

        /// <summary>
        /// Removes all entries for this collection in the index cache used by EnsureIndex. Call this method
        /// when you know (or suspect) that a process other than this one may have dropped one or
        /// more indexes.
        /// </summary>
        public virtual void ResetIndexCache()
        {
            _server.IndexCache.Reset(this);
        }

        /// <summary>
        /// Saves a document to this collection. The document must have an identifiable Id field. Based on the value
        /// of the Id field Save will perform either an Insert or an Update.
        /// </summary>
        /// <param name="nominalType">The type of the document to save.</param>
        /// <param name="document">The document to save.</param>
        /// <param name="options">The options to use for this Save.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        protected SafeModeResult SaveHelper(Type nominalType, object document, MongoInsertOptions options)
        {
            if (nominalType == null)
            {
                throw new ArgumentNullException("nominalType");
            }
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var serializer = BsonSerializer.LookupSerializer(document.GetType());
            var idProvider = serializer as IBsonIdProvider;
            object id;
            Type idNominalType;
            IIdGenerator idGenerator;
            if (idProvider != null && idProvider.GetDocumentId(document, out id, out idNominalType, out idGenerator))
            {
                if (id == null && idGenerator == null)
                {
                    throw new InvalidOperationException("No IdGenerator found.");
                }

                if (idGenerator != null && idGenerator.IsEmpty(id))
                {
                    id = idGenerator.GenerateId(this, document);
                    idProvider.SetDocumentId(document, id);
                    var result = InsertBatchHelper(nominalType, new object[] { document }, options);
                    return (result == null) ? null : result.Single();
                }
                else
                {
                    BsonValue idBsonValue;
                    var documentType = document.GetType();
                    if (BsonClassMap.IsClassMapRegistered(documentType))
                    {
                        var classMap = BsonClassMap.LookupClassMap(documentType);
                        var idMemberMap = classMap.IdMemberMap;
                        var idSerializer = idMemberMap.GetSerializer(id.GetType());
                        // we only care about the serialized _id value but we need a dummy document to serialize it into
                        var bsonDocument = new BsonDocument();
                        var bsonDocumentWriterSettings = new BsonDocumentWriterSettings
                        {
                            GuidRepresentation = _settings.GuidRepresentation.Value
                        };
                        var bsonWriter = BsonWriter.Create(bsonDocument, bsonDocumentWriterSettings);
                        bsonWriter.WriteStartDocument();
                        bsonWriter.WriteName("_id");
                        idSerializer.Serialize(bsonWriter, id.GetType(), id, idMemberMap.SerializationOptions);
                        bsonWriter.WriteEndDocument();
                        idBsonValue = bsonDocument[0]; // extract the _id value from the dummy document
                    } else {
                        if (!BsonTypeMapper.TryMapToBsonValue(id, out idBsonValue))
                        {
                            idBsonValue = BsonDocumentWrapper.Create(idNominalType, id);
                        }
                    }

                    var query = Query.EQ("_id", idBsonValue);
                    var update = MongoDB.Driver.Update.Replace(nominalType, document);
                    var updateOptions = new MongoUpdateOptions
                    {
                        CheckElementNames = options.CheckElementNames,
                        Flags = UpdateFlags.Upsert,
                        SafeMode = options.SafeMode
                    };
                    return Update(query, update, updateOptions);
                }
            }
            else
            {
                throw new InvalidOperationException("Save can only be used with documents that have an Id.");
            }
        }

        /// <summary>
        /// Gets a canonical string representation for this database.
        /// </summary>
        /// <returns>A canonical string representation for this database.</returns>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Updates one matching document in this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="update">The update to perform on the matching document.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Update(IMongoQuery query, IMongoUpdate update)
        {
            var options = new MongoUpdateOptions();
            return Update(query, update, options);
        }

        /// <summary>
        /// Updates one or more matching documents in this collection (for multiple updates use UpdateFlags.Multi).
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="update">The update to perform on the matching document.</param>
        /// <param name="options">The update options.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Update(IMongoQuery query, IMongoUpdate update, MongoUpdateOptions options)
        {
            var updateBuilder = update as UpdateBuilder;
            if (updateBuilder != null)
            {
                if (updateBuilder.Document.ElementCount == 0)
                {
                    throw new ArgumentException("Update called with an empty UpdateBuilder that has no update operations.");
                }
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var connection = _server.AcquireConnection(_database, ReadPreference.Primary);
            try
            {
                var writerSettings = GetWriterSettings(connection);
                using (var message = new MongoUpdateMessage(writerSettings, FullName, options.CheckElementNames, options.Flags, query, update))
                {
                    var safeMode = options.SafeMode ?? _settings.SafeMode;
                    return connection.SendMessage(message, safeMode, _database.Name);
                }
            }
            finally
            {
                _server.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Updates one matching document in this collection.
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="update">The update to perform on the matching document.</param>
        /// <param name="safeMode">The SafeMode to use for this operation.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Update(IMongoQuery query, IMongoUpdate update, SafeMode safeMode)
        {
            var options = new MongoUpdateOptions { SafeMode = safeMode };
            return Update(query, update, options);
        }

        /// <summary>
        /// Updates one or more matching documents in this collection (for multiple updates use UpdateFlags.Multi).
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="update">The update to perform on the matching document.</param>
        /// <param name="flags">The flags for this Update.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Update(IMongoQuery query, IMongoUpdate update, UpdateFlags flags)
        {
            var options = new MongoUpdateOptions { Flags = flags };
            return Update(query, update, options);
        }

        /// <summary>
        /// Updates one or more matching documents in this collection (for multiple updates use UpdateFlags.Multi).
        /// </summary>
        /// <param name="query">The query (usually a QueryDocument or constructed using the Query builder).</param>
        /// <param name="update">The update to perform on the matching document.</param>
        /// <param name="flags">The flags for this Update.</param>
        /// <param name="safeMode">The SafeMode to use for this operation.</param>
        /// <returns>A SafeModeResult (or null if SafeMode is not being used).</returns>
        public virtual SafeModeResult Update(
            IMongoQuery query,
            IMongoUpdate update,
            UpdateFlags flags,
            SafeMode safeMode)
        {
            var options = new MongoUpdateOptions
            {
                Flags = flags,
                SafeMode = safeMode
            };
            return Update(query, update, options);
        }

        /// <summary>
        /// Validates the integrity of this collection.
        /// </summary>
        /// <returns>A <see cref="ValidateCollectionResult"/>.</returns>
        public virtual ValidateCollectionResult Validate()
        {
            var command = new CommandDocument("validate", _name);
            return RunCommand<ValidateCollectionResult>(command);
        }

        // internal methods
        internal BsonBinaryReaderSettings GetReaderSettings(MongoConnection connection)
        {
            return new BsonBinaryReaderSettings
            {
                GuidRepresentation = _settings.GuidRepresentation.Value,
                MaxDocumentSize = connection.ServerInstance.MaxDocumentSize
            };
        }

        internal BsonBinaryWriterSettings GetWriterSettings(MongoConnection connection)
        {
            return new BsonBinaryWriterSettings
            {
                GuidRepresentation = _settings.GuidRepresentation.Value,
                MaxDocumentSize = connection.ServerInstance.MaxDocumentSize
            };
        }

        // private methods
        private string GetIndexName(BsonDocument keys, BsonDocument options)
        {
            if (options != null)
            {
                if (options.Contains("name"))
                {
                    return options["name"].AsString;
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (var element in keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("_");
                }
                sb.Append(element.Name);
                sb.Append("_");
                var value = element.Value;
                string valueString;
                switch (value.BsonType)
                {
                    case BsonType.Int32: valueString = ((BsonInt32)value).Value.ToString(); break;
                    case BsonType.Int64: valueString = ((BsonInt64)value).Value.ToString(); break;
                    case BsonType.Double: valueString = ((BsonDouble)value).Value.ToString(); break;
                    case BsonType.String: valueString = ((BsonString)value).Value; break;
                    default: valueString = "x"; break;
                }
                sb.Append(valueString.Replace(' ', '_'));
            }
            return sb.ToString();
        }

        private string GetIndexName(string[] keyNames)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string name in keyNames)
            {
                if (sb.Length > 0)
                {
                    sb.Append("_");
                }
                sb.Append(name);
                sb.Append("_1");
            }
            return sb.ToString();
        }

        private TCommandResult RunCommand<TCommandResult>(IMongoCommand command)
            where TCommandResult : CommandResult, new()
        {
            return (TCommandResult)RunCommand(typeof(TCommandResult), command);
        }

        private CommandResult RunCommand(IMongoCommand command)
        {
            return RunCommand<CommandResult>(command);
        }

        private CommandResult RunCommand(Type commandResultType, IMongoCommand command)
        {
            var commandCollectionSettings = new MongoCollectionSettings
            {
                AssignIdOnInsert = false,
                GuidRepresentation = _settings.GuidRepresentation,
                ReadPreference = _settings.ReadPreference,
                SafeMode = _settings.SafeMode
            };
            var commandCollection = _database.GetCollection("$cmd", commandCollectionSettings);

            return _database.RunCommandHelper(commandResultType, commandCollection, command);
        }
    }
}
