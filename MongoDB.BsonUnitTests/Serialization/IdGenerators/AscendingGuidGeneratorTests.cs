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
        public void TestGuid()
        {
            var expectedTicks = DateTime.UtcNow.Ticks;
            var expectedIncrement = 1000;
            var guid = (Guid)((AscendingGuidGenerator)_generator).
                             GenerateId (null,
                                         null, 
                                         expectedTicks, 
                                         expectedIncrement);
            var actualTicks = GetTicks(guid);
            var actualIncrement = GetIncrement(guid);
            Assert.AreEqual (expectedTicks, actualTicks);
            Assert.AreEqual (expectedIncrement, actualIncrement);
        }

        private long GetTicks(Guid guid)
        {
            var bytes = guid.ToByteArray();
            var a = BitConverter.ToInt32(bytes, 0);
            var b = BitConverter.ToInt16(bytes, 4);
            var c = BitConverter.ToInt16(bytes, 6);
            long tick = ((long)a << 32) + 
                ((long)b << 16) + 
                (long)c;
            return tick;
        }

        private int GetIncrement(Guid guid)
        {
            var bytes = guid.ToByteArray();
            int increment = ((int)bytes[13] << 16) + 
                ((int) bytes[14] << 8) +
                (int)bytes[15];
            return increment;
        }

    }
}
