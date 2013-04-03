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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MongoDB.Bson.Serialization.IdGenerators
{
    /// <summary>
    /// A GUID generator that generates GUIDs in ascending order. To enable 
    /// an index to make use of the ascending nature make sure to use
    /// <see cref="GuidRepresentation.Standard">GuidRepresentation.Standard</see>
    /// as the storage representation.
    /// Internally the GUID is of the form
    /// 8 bytes: Ticks from DateTime.UtcNow.Ticks
    /// 3 bytes: hash of machine name
    /// 2 bytes: low order bytes of process Id
    /// 3 bytes: increment
    /// </summary>
    public class AscendingGuidGenerator : IIdGenerator
    {
        // private static fields
        private static AscendingGuidGenerator __instance = new AscendingGuidGenerator();
        private static byte[] __machineProcessId;
        private static int __increment; 

        // static constructor
        static AscendingGuidGenerator()
        {
            int machineId = GetMachineHash();
            short processId;

            try
            {
                // use low order two bytes only
                processId = (short)GetCurrentProcessId(); 
            }
            catch (SecurityException)
            {
                processId = 0;
            }

            __machineProcessId = new byte[5];
            __machineProcessId[0] = (byte)(machineId >> 16);
            __machineProcessId[1] = (byte)(machineId >> 8);
            __machineProcessId[2] = (byte)(machineId);
            __machineProcessId[3] = (byte)(processId >> 8);
            __machineProcessId[4] = (byte)(processId);
        }

        // public static properties

        /// <summary>
        /// Gets an instance of AscendingGuidGenerator.
        /// </summary>
        public static AscendingGuidGenerator Instance
        {
            get { return __instance; }
        }
		
		// public methods

        /// <summary>
        /// Generates an ascending Guid for a document. Consecutive invocations
        /// should generate Guids that are ascending from a MongoDB perspective
        /// </summary>
        /// <param name="container">The container of the document (will be a 
        /// MongoCollection when called from the driver). </param>
        /// <param name="document">The document it was generated for.</param>
        /// <returns>A Guid.</returns>
        public object GenerateId(object container, object document)
        {
            int increment = Interlocked.Increment(ref __increment) &
                0x00ffffff;
            return GenerateId(DateTime.UtcNow.Ticks,
                              __machineProcessId,
                              increment);
        }

        /// <summary>
        /// Generates a Guid for a document. Note - this is purely used for
        /// unit testing
        /// </summary>
        /// <param name="tickCount">The time portion of the Guid</param>
        /// <param name="machineProcess">A 5 byte array with the first 3 bytes
        /// representing a machine id and the next 2 representing a process
        /// id</param>
        /// <param name="increment">The increment portion of the Guid. Used
        /// to distinguish between 2 Guids that have the timestamp. Note
        /// only the least significant 3 bytes are used.</param>
        /// <returns>A Guid.</returns>
        public object GenerateId(
		    long tickCount,
            byte[] machineProcessId,
            int increment
            )
        {
			int a = (int)(tickCount >> 32);
			short b = (short)(tickCount >> 16);
			short c = (short)(tickCount);

			byte[] d = new byte[8];
            Array.Copy(d, machineProcessId, 5);
            d[5] = (byte)(increment >> 16);
            d[6] = (byte)(increment >> 8);
            d[7] = (byte)(increment);
			return new Guid (a, b, c, d);
        }

        /// <summary>
        /// Tests whether an id is empty.
        /// </summary>
        /// <param name="id">The id to test.</param>
        /// <returns>True if the Id is empty. False otherwise</returns>
        public bool IsEmpty(object id)
        {
            return id == null || (Guid)id == Guid.Empty;
        }

        // private static methods
        /// <summary>
        /// Gets the current process id.  This method exists because of how
        /// CAS operates on the call stack, checking for permissions before
        /// executing the method.  Hence, if we inlined this call, the calling
        /// method would not execute before throwing an exception requiring the
        /// try/catch at an even higher level that we don't necessarily control.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        private static int GetMachineHash()
        {
            // use instead of Dns.HostName so it will work offline
            var hostName = Environment.MachineName; 
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(hostName));
            // use first 3 bytes of hash
            return (hash[0] << 16) + (hash[1] << 8) + hash[2]; 
        }

    }
}
