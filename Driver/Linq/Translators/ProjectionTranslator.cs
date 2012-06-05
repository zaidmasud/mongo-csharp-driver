using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq.Utils;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Translates an expression into a Projection for use in sending to the server.
    /// </summary>
    internal class ProjectionTranslator : ExpressionVisitor
    {
        private readonly BsonSerializationInfoHelper _serializationInfoHelper;
        private ParameterExpression _flattenedValuesParameter;
        private List<BsonSerializationInfo> _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionTranslator"/> class.
        /// </summary>
        public ProjectionTranslator()
        {
            _serializationInfoHelper = new BsonSerializationInfoHelper();
        }

        /// <summary>
        /// Translates the expression to a projection.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public Projection TranslateProjection(LambdaExpression expression)
        {
            var fieldSerializationInfoList = FieldCollector.CollectFields(_serializationInfoHelper, expression.Body);
            if (fieldSerializationInfoList.Count == 0)
            {
                return new Projection(
                    expression.Parameters[0].Type,
                    expression,
                    expression);
            }

            _fields = GetUniqueFieldHierarchy(fieldSerializationInfoList);

            _flattenedValuesParameter = Expression.Parameter(typeof(IDictionary<string, object>), "flattendValues");
            var transformed = Visit(expression.Body);

            var projector = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(_flattenedValuesParameter.Type, transformed.Type),
                transformed,
                _flattenedValuesParameter);

            return new Projection(
                _flattenedValuesParameter.Type,
                projector,
                expression,
                BuildMongoFields(),
                new ProjectionDeserializer(_fields));
        }

        protected override Expression Visit(Expression node)
        {
            BsonSerializationInfo fieldSerializationInfo;
            if (_serializationInfoHelper.TryGetSerializationInfo(node, out fieldSerializationInfo) && _fields.Any(x => x.ElementName == fieldSerializationInfo.ElementName))
            {
                return Expression.Convert(
                    Expression.Call(
                        typeof(ProjectionTranslator).GetMethod("GetValue", BindingFlags.Static | BindingFlags.NonPublic),
                        _flattenedValuesParameter,
                        Expression.Constant(fieldSerializationInfo.ElementName)),
                    node.Type);
            }
            return base.Visit(node);
        }

        private FieldsBuilder BuildMongoFields()
        {
            var mongoFields = new FieldsBuilder();

            if (!_fields.Any(x => x.ElementName == "_id"))
            {
                mongoFields = mongoFields.Exclude("_id");
            }

            return mongoFields.Include(_fields.Select(x => x.ElementName).Distinct().ToArray());
        }

        private static List<BsonSerializationInfo> GetUniqueFieldHierarchy(List<BsonSerializationInfo> fieldSerializationInfoList)
        {
            //we want to leave out subelements when the parent element exists. 
            //for instance, if we have asked for both "d" and "d.e", we only want to send { "d" : 1 } to the server.
            List<BsonSerializationInfo> fields = new List<BsonSerializationInfo>();
            foreach (var field in fieldSerializationInfoList.OrderBy(x => x.ElementName))
            {
                var lastIndexOfDot = field.ElementName.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    if (!fields.Contains(field))
                    {
                        fields.Add(field);
                    }
                }
                else
                {
                    var prefix = field.ElementName.Substring(0, lastIndexOfDot);
                    if (!fields.Any(x => x.ElementName == prefix))
                    {
                        fields.Add(field);
                    }
                }
            }

            return fields;
        }

        private static object GetValue(IDictionary<string, object> flattenedValues, string key)
        {
            object result;
            if (!flattenedValues.TryGetValue(key, out result))
                return null;

            return result;
        }

        /// <summary>
        /// Greedily collects all the known fields.
        /// </summary>
        private class FieldCollector : ExpressionVisitor
        {
            public static List<BsonSerializationInfo> CollectFields(BsonSerializationInfoHelper helper, Expression expression)
            {
                var collector = new FieldCollector(helper);
                collector.Visit(expression);
                return collector._fields;
            }

            private readonly List<BsonSerializationInfo> _fields;
            private readonly BsonSerializationInfoHelper _helper;

            private FieldCollector(BsonSerializationInfoHelper helper)
            {
                _helper = helper;
                _fields = new List<BsonSerializationInfo>();
            }

            protected override Expression Visit(Expression node)
            {
                BsonSerializationInfo serializationInfo;
                if (_helper.TryGetSerializationInfo(node, out serializationInfo))
                {
                    _fields.Add(serializationInfo);
                    return node;
                }
                return base.Visit(node);
            }
        }
    }
}