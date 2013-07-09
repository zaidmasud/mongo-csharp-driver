using System;
using NUnit.Framework;

namespace MongoDB.Driver.Core.Connections
{
    [TestFixture]
    public class FeatureTableTests
    {
        private FeatureTable _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new FeatureTable();

            _subject.AddFeature("One", new Version(1, 1));
            _subject.AddFeature("Two", new Version(1, 1), new Version(2, 0));
        }

        [Test]
        public void Supports_should_return_false_when_version_is_too_low()
        {
            var result1 = _subject.Supports("One", new Version(1, 0));
            var result2 = _subject.Supports("Two", new Version(1, 0));

            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [Test]
        public void Supports_should_return_true_when_version_is_equal_to_the_minumum()
        {
            var result1 = _subject.Supports("One", new Version(1, 1));
            var result2 = _subject.Supports("Two", new Version(1, 1));

            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
        }

        [Test]
        public void Supports_should_return_true_when_version_is_greater_than_the_minimum()
        {
            var result = _subject.Supports("One", new Version(10,0));

            Assert.IsTrue(result);
        }

        [Test]
        public void Supports_should_return_true_when_version_is_greater_than_the_minimum_and_below_the_maximum()
        {
            var result = _subject.Supports("Two", new Version(1, 1, 2));

            Assert.IsTrue(result);
        }

        [Test]
        public void Supports_should_return_false_when_version_is_equal_to_the_maximum()
        {
            var result = _subject.Supports("Two", new Version(2, 0));

            Assert.IsFalse(result);
        }

        [Test]
        public void Supports_should_return_false_when_version_is_greater_than_the_maximum()
        {
            var result = _subject.Supports("Two", new Version(10, 0));

            Assert.IsFalse(result);
        }
        
    }
}