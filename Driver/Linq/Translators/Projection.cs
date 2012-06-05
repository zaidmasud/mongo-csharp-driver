using System;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Represents a Projection.
    /// </summary>
    public class Projection
    {
        private readonly Type _documentType;
        private readonly IBsonSerializer _serializer;
        private readonly IMongoFields _mongoFields;
        private readonly LambdaExpression _projector;
        private readonly LambdaExpression _originalProjector;

        /// <summary>
        /// Gets the type of the document that will be sent into the projector.
        /// </summary>
        public Type DocumentType
        {
            get { return _documentType; }
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
        internal Projection(Type documentType, LambdaExpression projector, LambdaExpression originalProjector, IMongoFields mongoFields, IBsonSerializer serializer)
            : this(documentType, projector, originalProjector)
        {
            _mongoFields = mongoFields;
            _serializer = serializer;
        }
    }
}