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
using System.Reflection;
using System.Text;

using MongoDB.Bson.Serialization;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq.Utils;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Translates an expression into a Projection that works with partial documents returned from the server with only the needed fields.
    /// </summary>
    internal class ProjectionTranslator : ExpressionVisitor
    {
        // private fields
        private readonly BsonSerializationInfoHelper _serializationInfoHelper;
        private List<BsonSerializationInfo> _fields;
        private ParameterExpression _flattenedValuesParameter;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionTranslator"/> class.
        /// </summary>
        private ProjectionTranslator(
            BsonSerializationInfoHelper serializationInfoHelper,
            List<BsonSerializationInfo> fields,
            ParameterExpression flattenedValuesParameter)
        {
            _serializationInfoHelper = serializationInfoHelper;
            _fields = fields;
            _flattenedValuesParameter = flattenedValuesParameter;
        }

        // public static methods
        /// <summary>
        /// Translates the expression to a projection.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static Projection TranslateProjection(LambdaExpression expression)
        {
            var serializationInfoHelper = new BsonSerializationInfoHelper();
            var usedFields = FieldCollector.CollectFields(serializationInfoHelper, expression.Body);
            if (usedFields.Count == 0)
            {
                return new Projection(
                    expression.Parameters[0].Type,
                    expression,
                    expression);
            }

            var uniqueFields = GetUniqueFieldHierarchy(usedFields);
            var flattenedValuesParameter = Expression.Parameter(typeof(IDictionary<string, object>), "flattenedValues");
            var projectionTranslator = new ProjectionTranslator(serializationInfoHelper, uniqueFields, flattenedValuesParameter);
            var newProjectorBody = projectionTranslator.Visit(expression.Body);

            var newProjector = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(flattenedValuesParameter.Type, newProjectorBody.Type),
                newProjectorBody,
                flattenedValuesParameter);

            var serializer = new ProjectionDeserializer(uniqueFields);
            var mongoFields = BuildMongoFields(uniqueFields);

            return new Projection(
                flattenedValuesParameter.Type,
                newProjector,
                expression, // originalProjector
                serializer,
                mongoFields);
        }

        // private static methods
        private static IMongoFields BuildMongoFields(List<BsonSerializationInfo> fields)
        {
            var fieldsBuilder = new FieldsBuilder();

            if (!fields.Any(x => x.ElementName == "_id"))
            {
                fieldsBuilder = fieldsBuilder.Exclude("_id");
            }
            fieldsBuilder.Include(fields.Select(x => x.ElementName).Distinct().ToArray());

            return fieldsBuilder;
        }

        private static List<BsonSerializationInfo> GetUniqueFieldHierarchy(List<BsonSerializationInfo> fieldSerializationInfoList)
        {
            // we want to leave out subelements when the parent element exists
            // for instance, if we have asked for both "d" and "d.e", we only want to send { "d" : 1 } to the server
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

        // WARNING: this method is not called directly but is called by lambdas that are built at runtime
        private static object GetValue(IDictionary<string, object> flattenedValues, string key)
        {
            object result;
            flattenedValues.TryGetValue(key, out result); // result set to null if key not found
            return result;
        }

        // protected methods
        protected override Expression Visit(Expression node)
        {
            BsonSerializationInfo fieldSerializationInfo;
            if (_serializationInfoHelper.TryGetSerializationInfo(node, out fieldSerializationInfo))
            {
                if (_fields.Any(x => x.ElementName == fieldSerializationInfo.ElementName))
                {
                    return Expression.Convert(
                        Expression.Call(
                            typeof(ProjectionTranslator).GetMethod("GetValue", BindingFlags.Static | BindingFlags.NonPublic),
                            _flattenedValuesParameter,
                            Expression.Constant(fieldSerializationInfo.ElementName)),
                        node.Type);
                }
            }
            return base.Visit(node);
        }

        // private methods
        /// <summary>
        /// Greedily collects all the known fields.
        /// </summary>
        private class FieldCollector : ExpressionVisitor
        {
            // private fields
            private readonly BsonSerializationInfoHelper _serializationInfoHelper;
            private readonly List<BsonSerializationInfo> _fields;

            // constructors
            private FieldCollector(BsonSerializationInfoHelper serializationInfoHelper)
            {
                _serializationInfoHelper = serializationInfoHelper;
                _fields = new List<BsonSerializationInfo>();
            }

            // public static methods
            public static List<BsonSerializationInfo> CollectFields(BsonSerializationInfoHelper serializationInfoHelper, Expression expression)
            {
                var collector = new FieldCollector(serializationInfoHelper);
                collector.Visit(expression);
                return collector._fields;
            }

            // protected methods
            protected override Expression Visit(Expression node)
            {
                BsonSerializationInfo serializationInfo;
                if (_serializationInfoHelper.TryGetSerializationInfo(node, out serializationInfo))
                {
                    _fields.Add(serializationInfo);
                    return node;
                }
                return base.Visit(node);
            }
        }
    }
}