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

using NUnit.Framework;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoDB.BsonUnitTests.Serialization
{
    [TestFixture]
    public class AscendingGuidGeneratorTests
    {
        private IIdGenerator _generator = new AscendingGuidGenerator();

        [Test]
        public void TestIsEmpty()
        {
            Assert.IsTrue(_generator.IsEmpty(null));
            Assert.IsTrue(_generator.IsEmpty(Guid.Empty));
            var guid = _generator.GenerateId(null, null);
            Assert.IsFalse(_generator.IsEmpty(guid));
        }

        [Test]
        public void TestAscending()
        {
            var startTime = DateTime.UtcNow.Ticks;
            var guid1 = (Guid) _generator.GenerateId(null, null);
            var guid2 = (Guid) _generator.GenerateId(null, null);
            var endTime = DateTime.UtcNow.Ticks;
            var ts1 = GetTicks(guid1);
            var ts2 = GetTicks(guid2);
            Assert.IsTrue(startTime <= ts1);
            Assert.IsTrue(ts1 <= ts2);
            Assert.IsTrue(ts2 <= endTime);
        }

        private long GetTicks(Guid guid)
        {
            var bytes = guid.ToByteArray();
            long tick = ((long)bytes[0] << 56) + ((long) bytes[1] << 48)
                + ((long) bytes[2] << 40) + ((long)bytes[3] << 32)
                + ((long) bytes[4] << 24) + ((long)bytes[5] << 16)
                + ((long) bytes[6] << 8) + (long)bytes[7];
            return tick;
        }

    }
}
