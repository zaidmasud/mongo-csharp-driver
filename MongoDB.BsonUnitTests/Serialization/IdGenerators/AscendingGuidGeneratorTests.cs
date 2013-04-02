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
            long expectedTicks = DateTime.Now.Ticks;
            var expectedIncrement = 1000;
            var expectedMachineId = 1234;
            short expectedProcessId = 4321;
            var guid = (Guid)((AscendingGuidGenerator)_generator).
                             GenerateId (null,
                                         null, 
                                         expectedTicks,
                                         expectedMachineId,
                                         expectedProcessId,
                                         expectedIncrement);
            var bytes = guid.ToByteArray();
            var actualTicks = GetTicks(bytes);
            var actualMachineId = GetMachineId(bytes);
            var actualProcessId = GetProcessId(bytes);
            var actualIncrement = GetIncrement(bytes);
            Assert.AreEqual(expectedTicks, actualTicks);
            Assert.AreEqual(expectedMachineId, actualMachineId);
            Assert.AreEqual(expectedProcessId, actualProcessId);
            Assert.AreEqual(expectedIncrement, actualIncrement);
        }

        private long GetTicks(byte[] bytes)
        {   
            var a = (long) BitConverter.ToInt32(bytes, 0);
            var b = (long) BitConverter.ToInt16(bytes, 4);
            var c = (long) BitConverter.ToInt16(bytes, 6);
            long tick = (a << 32) | ((b << 16) & 0xFFFF0000) |
                (c & 0xFFFF);
            return tick;
        }

        private int GetMachineId(byte[] bytes)
        {
            int machineId = ((int)bytes[8] << 16) +
                ((int)bytes[9] << 8) +
                (int)bytes[10];
            return machineId;
        }

        private short GetProcessId(byte[] bytes)
        {
            short processId = (short) ((bytes[11] << 8) + (bytes[12]));
            return processId;
        }

        private int GetIncrement(byte[] bytes)
        {
            int increment = ((int)bytes[13] << 16) +
                ((int)bytes[14] << 8) +
                (int)bytes[15];
            return increment;
        }
    }
}
