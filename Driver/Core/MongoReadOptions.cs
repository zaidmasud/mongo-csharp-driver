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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the options to use for a Read operation.
    /// </summary>
    public class MongoReadOptions
    {
        // private fields
        private ReadPreference _readPreference;

        // constructors
        /// <summary>
        /// Initializes a new instance of the MongoReadOptions class.
        /// </summary>
        public MongoReadOptions()
        {
        }

        // public properties
        /// <summary>
        /// Gets or sets the ReadPreference to use for the Read operation.
        /// </summary>
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
            set { _readPreference = value; }
        }
    }
}
