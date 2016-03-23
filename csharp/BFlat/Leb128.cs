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
    ///<summary>
    /// Implementation of the LEB128 integer format which is
    /// used by BFlat to represent lengths and integers.
    ///</summary>
    public class Leb128
    {
        private const int bits_per_digit = 7;

        /// <summary>
        /// Decodes a signed LEB128 value.
        /// </summary>
        /// <param name="source">The source bytes for the value.</param>
        /// <returns>The decoded value.</returns>
        public static long decodeSigned(Buffer source)
        {
            try
            {
                long result = 0;
                int shift = 0;
                while ((source.data[source.position] & 0x80) != 0)
                {
                    long data = (source.data[source.position++] & 0x7f) << shift;
                    shift += bits_per_digit;
                    result |= data;
                }
                result |= unchecked((long)(source.data[source.position] << shift));
                if ((source.data[source.position++] & 0x40) != 0)
                    result -= (1 << (shift + bits_per_digit));
                return result;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BFlatException("invalid signed leb128 value", source.position);
            }
        }

        /// <summary>
        /// Encodes a signed LEB128 value.
        /// </summary>
        /// <param name="output">The output buffer for the encoded value.
        ///   </param>
        /// <param name="value">The value to encode.</param>
        /// <returns>The number of bytes written to in
        ///  <paramref name="output"/>.</returns>
        public static int encodeSigned(Buffer output, long value)
        {
            try
            {
                bool isMore = true;
                int start = output.position;
                while (isMore)
                {
                    byte thisByte = (byte)(value & 0x7f);
                    value >>= 7;
                    isMore = !((((value == 0) && ((thisByte & 0x40) == 0)) ||
                                ((value == -1) && ((thisByte & 0x40) != 0))));
                    if (isMore) thisByte |= 0x80;
                    output.data[output.position++] = thisByte;
                }
                return output.position - start;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BufferTooSmallException("out of buffer while encoding leb128");
            }
        }

        /// <summary>
        /// Decodes an unsigned LEB128 value.
        /// </summary>
        /// <param name="source">The source bytes for the value.</param>
        /// <returns>The decoded value.</returns>
        public static ulong decodeUnsigned(Buffer source)
        {
            try
            {
                ulong result = 0;
                int shift = 0;
                while ((source.data[source.position] & 0x80) != 0)
                {
                    ulong data = unchecked((ulong)(source.data[source.position++] & 0x7f)) << shift;
                    shift += bits_per_digit;
                    result |= data;
                }
                result |= unchecked((ulong)source.data[source.position++]) << shift;
                return result;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BFlatException("invalid unsigned leb128 value", source.position);
            }
        }

        /// <summary>
        /// Encodes an unsigned LEB128 value.
        /// </summary>
        /// <param name="output">The output buffer for the encoded value.
        ///   </param>
        /// <param name="value">The value to encode.</param>
        /// <returns>The number of bytes written to in
        ///  <paramref name="output"/>.</returns>
        public static int encodeUnsigned(Buffer output, ulong value)
        {
            try
            {
                bool isMore = true;
                int start = output.position;
                while (isMore)
                {
                    byte thisByte = (byte)(value & 0x7f);
                    value >>= 7;
                    isMore = value != 0;
                    if (isMore) thisByte |= 0x80;
                    output.data[output.position++] = thisByte;
                }
                return output.position - start;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BufferTooSmallException("out of buffer while encoding leb128");
            }
        }
    }
}
