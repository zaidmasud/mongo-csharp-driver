using System;
using NUnit.Framework;

namespace MongoDB.Driver.Core.Connections
{
    [TestFixture]
    public class FeatureTableTests
    {
        private Version[] _versions = new Version[]
        {
            new Version(2, 0),
            new Version(2, 1),
            new Version(2, 2),
            new Version(2, 3),
            new Version(2, 4),
            new Version(2, 5)
        };

        [Test]
        [TestCase("AggregationFramework", "2.1", "9999.0")]
        public void TestIsFeatureSupported(string name, string fromString, string toString)
        {
            var from = new Version(fromString);
            var to = new Version(toString);
            foreach (var version in _versions)
            {
                var expected = version >= from && version < to;
                var featureTable = FeatureTable.FromServerVersion(version);
                Assert.AreEqual(expected, featureTable.IsFeatureSupported(name));
            }
        }
    }
}
