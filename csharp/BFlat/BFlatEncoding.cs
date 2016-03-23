////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2016 60East Technologies Inc., All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFlat
{
    /// <summary>
    /// BFlatEncoding contains constant values specific to the BFlat encoding.
    /// Values from this class are used to specify data types when using
    /// <see cref="BFlatValue"/> and <see cref="BFLatBuilder"/>.
    /// </summary>
    public class BFlatEncoding
    {
        internal const byte TypeMask = 0x78;
        internal const byte LengthMask = 0x07;
        internal const byte ArrayMask = 0x80;
        internal const int LengthBits = 3;

        /// <summary>
        /// Type is an enum of allowed BFlat type codes.
        /// </summary>
        public enum Type : byte
        {
            /// <summary>
            /// Represents a NULL value in BFlat.
            /// </summary>
            Null = 0x00,
            /// <summary>
            /// Represents a String value in BFlat.
            /// </summary>
            String = 0x01 << LengthBits,
            /// <summary>
            /// Represents a binary value in BFlat.
            /// </summary>
            Binary = 0x02 << LengthBits,
            /// <summary>
            /// Represents an 8-bit-signed integer in BFlat.
            /// </summary>
            Int8 = 0x03 << LengthBits,
            /// <summary>
            /// Represents a 16-bit signed integer in BFLat.
            /// </summary>
            Int16 = 0x04 << LengthBits,
            /// <summary>
            /// Represents a 32-bit signed integer in BFlat.
            /// </summary>
            Int32 = 0x05 << LengthBits,
            /// <summary>
            /// Repersents a 64-bit signed integer in BFlat.
            /// </summary>
            Int64 = 0x06 << LengthBits,
            /// <summary>
            /// Represents a IEEE-754 double-precision value in BFlat.
            /// </summary>
            Double = 0x07 << LengthBits,
            /// <summary>
            /// Reperesents a 64-bit integer datetime value in BFlat.
            /// </summary>
            Datetime = 0x08 << LengthBits,
            /// <summary>
            /// Represents a variable length signed integer value up to
            /// 128 bits in precision, in BFlat.
            /// </summary>
            Leb128 = 0x09 << LengthBits
        }
    }
}
