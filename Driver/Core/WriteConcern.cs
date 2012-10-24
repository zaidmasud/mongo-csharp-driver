/* Copyright 2010-2012 10gen Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the different WriteConcerns that can be used.
    /// </summary>
    [Serializable]
    public class WriteConcern : IEquatable<WriteConcern>
    {
        // private static fields
        private readonly static WriteConcern __errors = new WriteConcern { Enabled = true }.Freeze();
        private readonly static WriteConcern __fsyncTrue = new WriteConcern { Enabled = true, FSync = true }.Freeze();
        private readonly static WriteConcern __journalTrue = new WriteConcern { Enabled = true, Journal = true }.Freeze();
        private readonly static WriteConcern __majority = new WriteConcern { Enabled = true, WMode = "majority" }.Freeze();
        private readonly static WriteConcern __networkErrorsOnly = new WriteConcern { Enabled = false }.Freeze();
        private readonly static WriteConcern __none = new WriteConcern { Enabled = false, IgnoreNetworkErrors = true }.Freeze();
        private readonly static WriteConcern __w2 = new WriteConcern { Enabled = true, W = 2 }.Freeze();
        private readonly static WriteConcern __w3 = new WriteConcern { Enabled = true, W = 3 }.Freeze();
        private readonly static WriteConcern __w4 = new WriteConcern { Enabled = true, W = 4 }.Freeze();

        // private fields
        private bool _enabled;
        private bool _fsync;
        private bool _ignoreNetworkErrors;
        private bool _journal;
        private int _w;
        private string _wmode;
        private TimeSpan _wtimeout;

        private bool _isFrozen;
        private int _frozenHashCode;

        // constructors
        /// <summary>
        /// Initializes a new instance of the WriteConcern class.
        /// </summary>
        public WriteConcern()
        {
            ResetValues();
        }

        // public static properties
        /// <summary>
        /// Gets an instance of WriteConcern that checks for errors.
        /// </summary>
        public static WriteConcern Errors
        {
            get { return __errors; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that waits for an fsync.
        /// </summary>
        public static WriteConcern FSyncTrue
        {
            get { return __fsyncTrue; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that waits for the journal to be written.
        /// </summary>
        public static WriteConcern JournalTrue
        {
            get { return __journalTrue; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern where w="majority".
        /// </summary>
        public static WriteConcern Majority
        {
            get { return __majority; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that checks for network errors only.
        /// </summary>
        public static WriteConcern NetworkErrorsOnly
        {
            get { return __networkErrorsOnly; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that doesn't check for any errors (not even network errors).
        /// </summary>
        public static WriteConcern None
        {
            get { return __none; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern where w=2.
        /// </summary>
        public static WriteConcern W2
        {
            get { return __w2; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern where w=3.
        /// </summary>
        public static WriteConcern W3
        {
            get { return __w3; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern where w=4.
        /// </summary>
        public static WriteConcern W4
        {
            get { return __w4; }
        }

        // public properties
        /// <summary>
        /// Gets or sets whether WriteConcern is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
                if (!value) { ResetValues(); }
                _enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to wait for an fsync to complete.
        /// </summary>
        public bool FSync
        {
            get { return _fsync; }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
                _fsync = value;
                _enabled |= value;
            }
        }

        /// <summary>
        /// Gets or sets whether to ignore network errors.
        /// </summary>
        public bool IgnoreNetworkErrors
        {
            get { return _ignoreNetworkErrors; }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
               _ignoreNetworkErrors = value;
            }
        }

        /// <summary>
        /// Gets whether this instance is frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return _isFrozen; }
        }

        /// <summary>
        /// Gets or sets whether to wait for journal commit.
        /// </summary>
        public bool Journal
        {
            get { return _journal; }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
                _journal = value;
                _enabled |= value;
            }
        }

        /// <summary>
        /// Gets or sets the w value (the number of write replications that must complete before the server returns).
        /// </summary>
        public int W
        {
            get { return _w; }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
                _w = value;
                _wmode = null;
                _enabled |= (value != 0);
            }
        }

        /// <summary>
        /// Gets or sets the w mode (the w mode determines which write replications must complete before the server returns).
        /// </summary>
        public string WMode
        {
            get { return _wmode; }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
                _w = 0;
                _wmode = value;
                _enabled |= (value != null);
            }
        }

        /// <summary>
        /// Gets or sets the wtimeout value (the timeout before which the server must return).
        /// </summary>
        public TimeSpan WTimeout
        {
            get { return _wtimeout; }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
                _wtimeout = value;
                _enabled |= true;
            }
        }

        // public operators
        /// <summary>
        /// Determines whether two specified WriteConcern objects have different values.
        /// </summary>
        /// <param name="lhs">The first value to compare, or null.</param>
        /// <param name="rhs">The second value to compare, or null.</param>
        /// <returns>True if the value of lhs is different from the value of rhs; otherwise, false.</returns>
        public static bool operator !=(WriteConcern lhs, WriteConcern rhs)
        {
            return !WriteConcern.Equals(lhs, rhs);
        }

        /// <summary>
        /// Determines whether two specified WriteConcern objects have the same value.
        /// </summary>
        /// <param name="lhs">The first value to compare, or null.</param>
        /// <param name="rhs">The second value to compare, or null.</param>
        /// <returns>True if the value of lhs is the same as the value of rhs; otherwise, false.</returns>
        public static bool operator ==(WriteConcern lhs, WriteConcern rhs)
        {
            return WriteConcern.Equals(lhs, rhs);
        }

        // public static methods
        /// <summary>
        /// Determines whether two specified WriteConcern objects have the same value.
        /// </summary>
        /// <param name="lhs">The first value to compare, or null.</param>
        /// <param name="rhs">The second value to compare, or null.</param>
        /// <returns>True if the value of lhs is the same as the value of rhs; otherwise, false.</returns>
        public static bool Equals(WriteConcern lhs, WriteConcern rhs)
        {
            if ((object)lhs == null) { return (object)rhs == null; }
            return lhs.Equals(rhs);
        }

        // public methods
        /// <summary>
        /// Creates a clone of the WriteConcern.
        /// </summary>
        /// <returns>A clone of the WriteConcern.</returns>
        public WriteConcern Clone()
        {
            var clone = new WriteConcern();
            clone._enabled = _enabled;
            clone._fsync = _fsync;
            clone._ignoreNetworkErrors = _ignoreNetworkErrors;
            clone._journal = _journal;
            clone._w = _w;
            clone._wmode = _wmode;
            clone._wtimeout = _wtimeout;
            return clone;
        }

        /// <summary>
        /// Determines whether this instance and a specified object, which must also be a WriteConcern object, have the same value.
        /// </summary>
        /// <param name="obj">The WriteConcern object to compare to this instance.</param>
        /// <returns>True if obj is a WriteConcern object and its value is the same as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as WriteConcern); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Determines whether this instance and another specified WriteConcern object have the same value.
        /// </summary>
        /// <param name="rhs">The WriteConcern object to compare to this instance.</param>
        /// <returns>True if the value of the rhs parameter is the same as this instance; otherwise, false.</returns>
        public bool Equals(WriteConcern rhs)
        {
            if ((object)rhs == null || GetType() != rhs.GetType()) { return false; }
            if ((object)this == (object)rhs) { return true; }
            return
                _enabled == rhs._enabled &&
                _fsync == rhs._fsync &&
                _ignoreNetworkErrors == rhs._ignoreNetworkErrors &&
                _journal == rhs._journal &&
                _w == rhs._w &&
                _wmode == rhs._wmode &&
                _wtimeout == rhs._wtimeout;
        }

        /// <summary>
        /// Freezes the WriteConcern.
        /// </summary>
        /// <returns>The frozen WriteConcern.</returns>
        public WriteConcern Freeze()
        {
            if (!_isFrozen)
            {
                _frozenHashCode = GetHashCode();
                _isFrozen = true;
            }
            return this;
        }

        /// <summary>
        /// Returns a frozen copy of the WriteConcern.
        /// </summary>
        /// <returns>A frozen copy of the WriteConcern.</returns>
        public WriteConcern FrozenCopy()
        {
            if (_isFrozen)
            {
                return this;
            }
            else
            {
                return Clone().Freeze();
            }
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            if (_isFrozen)
            {
                return _frozenHashCode;
            }

            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + _enabled.GetHashCode();
            hash = 37 * hash + _fsync.GetHashCode();
            hash = 37 * hash + _ignoreNetworkErrors.GetHashCode();
            hash = 37 * hash + _journal.GetHashCode();
            hash = 37 * hash + _w.GetHashCode();
            hash = 37 * hash + ((_wmode == null) ? 0 : _wmode.GetHashCode());
            hash = 37 * hash + _wtimeout.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the WriteConcern.
        /// </summary>
        /// <returns>A string representation of the WriteConcern.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("WriteConcern={0}", _enabled ? "true" : "false");
            if (_fsync)
            {
                sb.Append(",fsync=true");
            }
            if (_journal)
            {
                sb.Append(",journal=true");
            }
            if (_ignoreNetworkErrors)
            {
                sb.Append(",ignoreNetworkErrors=true");
            }
            if (_w != 0)
            {
                sb.AppendFormat(",w={0}", _w);
            }
            if (_wmode != null)
            {
                sb.AppendFormat(",wmode=\"{0}\"", _wmode);
            }
            if (_wtimeout != TimeSpan.Zero)
            {
                sb.AppendFormat(",wtimeout={0}", _wtimeout);
            }
            return sb.ToString();
        }

        // private methods
        private void ResetValues()
        {
            _enabled = true;
            _fsync = false;
            _journal = false;
            _w = 0;
            _wmode = null;
            _wtimeout = TimeSpan.Zero;
        }

        private void ThrowFrozenException()
        {
            throw new InvalidOperationException("WriteConcern has been frozen and no further changes are allowed.");
        }
    }
}
