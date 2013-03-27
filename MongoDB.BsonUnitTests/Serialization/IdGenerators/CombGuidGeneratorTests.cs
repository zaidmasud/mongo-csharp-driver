/* Copyright 2010-2013 10gen Inc.
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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using NUnit.Framework;

namespace MongoDB.BsonUnitTests.Serialization
{
    [TestFixture]
    public class CombGuidGeneratorTests
    {
        private IIdGenerator _generator = new CombGuidGenerator();

        [Test]
        public void TestGenerateId()
        {
            var guid1 = (Guid)_generator.GenerateId(null, null);
            var guid2 = (Guid)_generator.GenerateId(null, null);
            var ts1 = ExtractTimestamp(guid1);
            var ts2 = ExtractTimestamp(guid2);
            Assert.IsTrue(ts2 - ts1 < TimeSpan.FromMilliseconds(3.34));
        }

        [Test]
        public void TestIsEmpty()
        {
            Assert.IsTrue(_generator.IsEmpty(null));
            Assert.IsTrue(_generator.IsEmpty(Guid.Empty));
            Assert.IsFalse(_generator.IsEmpty(Guid.NewGuid()));
        }

        private TimeSpan ExtractTimestamp(Guid guid)
        {
            var bytes = guid.ToByteArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes, 10, 2);
                Array.Reverse(bytes, 12, 4);
            }
            var dayTicks = BitConverter.ToInt16(bytes, 10);
            var timeTicks = BitConverter.ToInt32(bytes, 12);
            return TimeSpan.FromDays(dayTicks) + TimeSpan.FromSeconds(timeTicks / 300);
        }
    }
}
