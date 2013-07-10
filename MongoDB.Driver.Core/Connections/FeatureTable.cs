using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Connections
{
    public class FeatureTable
    {
        // private static fields
        private static readonly ConcurrentDictionary<Version, FeatureTable> _versionFeatureTables = new ConcurrentDictionary<Version, FeatureTable>();

        // private fields
        private readonly Dictionary<string, bool> _features = new Dictionary<string, bool>();

        // constructors
        public FeatureTable(IEnumerable<Tuple<string, bool>> featureTuples)
        {
            foreach (var featureTuple in featureTuples)
            {
                _features.Add(featureTuple.Item1, featureTuple.Item2);
            }
        }

        // public static methods
        /// <summary>
        /// Creates a feature table for a specific server version.
        /// </summary>
        /// <param name="version">The server version.</param>
        /// <returns>A feature table.</returns>
        public static FeatureTable FromServerVersion(Version version)
        {
            Ensure.IsNotNull("version", version);
            return _versionFeatureTables.GetOrAdd(version, CreateFeatureTable);
        }

        // private static methods
        private static FeatureTable CreateFeatureTable(Version version)
        {
            var featureNames = Enum.GetValues(typeof(Feature)).Cast<Feature>().Select(f => f.ToString());
            var featureTuples = featureNames.Select(featureName => Tuple.Create(featureName, IsFeatureSupported(featureName, version)));
            return new FeatureTable(featureTuples);
        }

        private static bool IsFeatureSupported(string featureName, Version serverVersion)
        {
            switch (featureName)
            {
                case "AggregationFramework": return serverVersion >= new Version(2, 1);
                default: return false;
            }
        }

        // public methods
        /// <summary>
        /// Indicates whether the specified feature is supported.
        /// </summary>
        /// <param name="featureName">The feature name.</param>
        /// <returns><c>true</c> if the feature is supported for the server; otherwise <c>false</c>.</returns>
        public bool IsFeatureSupported(string featureName)
        {
            bool isFeatureSupported;
            if (_features.TryGetValue(featureName, out isFeatureSupported))
            {
                return isFeatureSupported;
            }
            else
            {
                var message = string.Format("Feature name '{0}' is not valid.", featureName);
                throw new ArgumentException(message, "featureName");
            }
        }
    }
}