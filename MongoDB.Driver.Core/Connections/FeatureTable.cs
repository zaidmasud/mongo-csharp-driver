using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Connections
{
    internal class FeatureTable
    {
        private readonly Dictionary<string, Feature> _table;

        public FeatureTable()
        {
            _table = new Dictionary<string, Feature>();
        }

        public FeatureTable AddFeature(string name, Version from)
        {
            _table.Add(name, new Feature(name, from));
            return this;
        }

        public FeatureTable AddFeature(string name, Version from, Version to)
        {
            _table.Add(name, new Feature(name, from, to));
            return this;
        }

        public bool Supports(string name, Version serverVersion)
        {
            Feature feature;
            if (!_table.TryGetValue(name, out feature))
            {
                throw new FeatureNotRegisteredException(name);
            }

            return feature.IsSupportedFor(serverVersion);
        }

        private class Feature
        {
            private readonly string _name;
            private readonly Version _from;
            private readonly Version _to;

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