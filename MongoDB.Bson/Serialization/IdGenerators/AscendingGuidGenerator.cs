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
    /// <seealso cref="CombGuidGenerator"/>
    /// <seealso cref="GuidGenerator"/>
    /// <seealso cref="ObjectIdGenerator"/>
    public class AscendingGuidGenerator : IIdGenerator
    {
        // private static fields
        private static AscendingGuidGenerator __instance = new AscendingGuidGenerator();
        private static int __staticMachine;
        private static short __staticPid;
        private static int __staticIncrement; 

        // static constructor
        static AscendingGuidGenerator()
        {
            __staticMachine = GetMachineHash();
            __staticIncrement = (new Random()).Next();

            try
            {
                __staticPid = (short) GetCurrentProcessId(); // use low order two bytes only
            }
            catch (SecurityException)
            {
                __staticPid = 0;
            }
        }

        // constructors
        /// <summary>
        /// Initializes a new instance of the AscendingGuidGenerator class.
        /// </summary>
        public AscendingGuidGenerator()
        {
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
        /// Generates an Id for a document.
        /// </summary>
        /// <param name="container">The container of the document (will be a 
        /// MongoCollection when called from the driver). </param>
        /// <param name="document">The document.</param>
        /// <returns>An Id.</returns>
        public object GenerateId(object container, object document)
        {
            int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff;
            if ((__staticMachine & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", 
                    "The machine value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            byte[] bytes = new byte[16];
            var currentTickCount = DateTime.UtcNow.Ticks;
            bytes[0] = (byte)(currentTickCount >> 56);
            bytes[1] = (byte)(currentTickCount >> 48);
            bytes[2] = (byte)(currentTickCount >> 40);
            bytes[3] = (byte)(currentTickCount >> 32);
            bytes[4] = (byte)(currentTickCount >> 24);
            bytes[5] = (byte)(currentTickCount >> 16);
            bytes[6] = (byte)(currentTickCount >> 8);
            bytes[7] = (byte)(currentTickCount);
            bytes[8] = (byte)(__staticMachine >> 16);
            bytes[9] = (byte)(__staticMachine >> 8);
            bytes[10] = (byte)(__staticMachine);
            bytes[11] = (byte)(__staticPid >> 8);
            bytes[12] = (byte)(__staticPid);
            bytes[13] = (byte)(increment >> 16);
            bytes[14] = (byte)(increment >> 8);
            bytes[15] = (byte)(increment);
            return new Guid(bytes);
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
            var hostName = Environment.MachineName; // use instead of Dns.HostName so it will work offline
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(hostName));
            return (hash[0] << 16) + (hash[1] << 8) + hash[2]; // use first 3 bytes of hash
        }

    }
}
