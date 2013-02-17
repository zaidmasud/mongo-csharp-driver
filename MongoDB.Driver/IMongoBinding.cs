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
using MongoDB.Driver.Internal;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a binding to a source of connections.
    /// </summary>
    public interface IMongoBinding
    {
        /// <summary>
        /// Gets a MongoCollection instance bound to this binding.
        /// </summary>
        /// <typeparam name="TDefaultDocument">The default document type for this collection.</typeparam>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        MongoCollection<TDefaultDocument> GetCollection<TDefaultDocument>(
            string databaseName,
            string collectionName);

        /// <summary>
        /// Gets a MongoCollection instance bound to this binding.
        /// </summary>
        /// <typeparam name="TDefaultDocument">The default document type for this collection.</typeparam>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="collectionSettings">The settings to use when accessing this collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        MongoCollection<TDefaultDocument> GetCollection<TDefaultDocument>(
            string databaseName,
            string collectionName,
            MongoCollectionSettings collectionSettings);

        /// <summary>
        /// Gets a MongoCollection instance bound to this binding.
        /// </summary>
        /// <param name="defaultDocumentType">The default document type.</param>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        MongoCollection GetCollection(
            Type defaultDocumentType,
            string databaseName,
            string collectionName);

        /// <summary>
        /// Gets a MongoCollection instance bound to this binding.
        /// </summary>
        /// <param name="defaultDocumentType">The default document type.</param>
        /// <param name="databaseName">The name of the database that contains the collection.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="collectionSettings">The settings to use when accessing this collection.</param>
        /// <returns>An instance of MongoCollection.</returns>
        MongoCollection GetCollection(
            Type defaultDocumentType,
            string databaseName,
            string collectionName,
            MongoCollectionSettings collectionSettings);

        /// <summary>
        /// Gets a connection from whatever this binding is bound to.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <returns>A MongoConnection.</returns>
        MongoConnection GetConnection(ReadPreference readPreference);

        /// <summary>
        /// Gets a MongoDatabase instance bound to this binding.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A MongoDatabase.</returns>
        MongoDatabase GetDatabase(string databaseName);

        /// <summary>
        /// Gets a MongoDatabase instance bound to this binding.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="databaseSettings">The settings to use with this database.</param>
        /// <returns>A MongoDatabase.</returns>
        MongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings databaseSettings);

        /// <summary>
        /// Releases an connection (for internal use only).
        /// </summary>
        /// <param name="connection">The connection.</param>
        void ReleaseConnection(MongoConnectionInternal connection);
    }
}
