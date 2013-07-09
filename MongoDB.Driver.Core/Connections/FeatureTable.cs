using System;
using System.Collections.Generic;

namespace MongoDB.Driver.Core.Connections
{
    internal class FeatureTable
    {
        // private fields
        private readonly Dictionary<string, Feature> _table;

        // constructors
        public FeatureTable()
        {
            _table = new Dictionary<string, Feature>();
        }

        // public methods
        /// <summary>
        /// Adds the feature.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="from">The initial version offering the feature.</param>
        /// <returns>The feature table for chaining.</returns>
        public FeatureTable AddFeature(string name, Version from)
        {
            _table.Add(name, new Feature(name, from));
            return this;
        }

        /// <summary>
        /// Adds the feature.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="from">The initial version offering the feature.</param>
        /// <param name="to">The version no longer offering the feature.</param>
        /// <returns>The feature table for chaining.</returns>
        public FeatureTable AddFeature(string name, Version from, Version to)
        {
            _table.Add(name, new Feature(name, from, to));
            return this;
        }

        /// <summary>
        /// Indicates whether the specified feature is supported for the specified version.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="serverVersion">The server version.</param>
        /// <returns><c>true</c> if the feature is supported for the server; otherwise <c>false</c>.</returns>
        public bool Supports(string name, Version serverVersion)
        {
            Feature feature;
            if (!_table.TryGetValue(name, out feature))
            {
                throw new FeatureNotRegisteredException(name);
            }

            return feature.IsSupportedFor(serverVersion);
        }

        // nested classes...
        private class Feature
        {
            private readonly string _name;
            private readonly Version _from; // inclusive
            private readonly Version _to;  //exclusive

            public Feature(string name, Version from)
                : this(name, from, null)
            { }

            public Feature(string name, Version from, Version to)
            {
                _name = name;
                _from = from;
                _to = to;
            }

            public bool IsSupportedFor(Version serverVersion)
            {
                return _from <= serverVersion && (_to == null || serverVersion < _to);
            }
        }
    }
}