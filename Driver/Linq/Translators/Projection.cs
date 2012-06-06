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
using System.Linq.Expressions;
using System.Text;

using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Represents a Projection.
    /// </summary>
    public class Projection
    {
        // private fields
        private readonly Type _documentType;
        private readonly LambdaExpression _projector;
        private readonly LambdaExpression _originalProjector;
        private readonly IBsonSerializer _serializer;
        private readonly IMongoFields _mongoFields;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Projection"/> class.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="projector">The projector.</param>
        /// <param name="originalProjector">The original projector.</param>
        internal Projection(Type documentType, LambdaExpression projector, LambdaExpression originalProjector)
        {
            _documentType = documentType;
            _projector = projector;
            _originalProjector = originalProjector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Projection"/> class.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="projector">The projector.</param>
        /// <param name="originalProjector">The original projector.</param>
        /// <param name="mongoFields">The mongo fields.</param>
        /// <param name="serializer">The serializer.</param>
        internal Projection(Type documentType, LambdaExpression projector, LambdaExpression originalProjector, IBsonSerializer serializer, IMongoFields mongoFields)
            : this(documentType, projector, originalProjector)
        {
            _serializer = serializer;
            _mongoFields = mongoFields;
        }

        // public properties
        /// <summary>
        /// Gets the type of the document that will be sent into the projector.
        /// </summary>
        public Type DocumentType
        {
            get { return _documentType; }
        }

        /// <summary>
        /// Gets the projector.
        /// </summary>
        public LambdaExpression Projector
        {
            get { return _projector; }
        }

        /// <summary>
        /// Gets the original projector.
        /// </summary>
        public LambdaExpression OriginalProjector
        {
            get { return _originalProjector; }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IBsonSerializer Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        /// Gets the mongo fields.
        /// </summary>
        public IMongoFields MongoFields
        {
            get { return _mongoFields; }
        }
    }
}