﻿/* Copyright 2010-2013 10gen Inc.
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

using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core
{
    /// <summary>
    /// Represents a database name.
    /// </summary>
    public class DatabaseNamespace
    {
        // private fields
        private readonly string _databaseName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseNamespace" /> class.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        public DatabaseNamespace(string databaseName)
        {
            Ensure.IsNotNull("databaseName", databaseName);

            _databaseName = databaseName;
        }

        // public properties
        /// <summary>
        /// Gets the name of the command collection for this database.
        /// </summary>
        public CollectionNamespace CommandCollection
        {
            get { return new CollectionNamespace(_databaseName, "$cmd"); }
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string DatabaseName
        {
            get { return _databaseName; }
        }
    }
}