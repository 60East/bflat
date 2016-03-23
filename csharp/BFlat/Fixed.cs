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
    /// A set of utility functions for encoding and decoding fixed-width integers
    /// </summary>
    public class Fixed
    {
        /// <summary>
        /// Encodes an 8-bit signed integer.
        /// </summary>
        /// <param name="value">The value to encode</param>
        /// <param name="output">The destination array</param>
        /// <param name="position">The posiiton in <paramref name="output"/> to write to.</param>
        /// <returns>The number of bytes written to <paramref name="output"/></returns>
        public static int encodeInt8(sbyte value, byte[] output, int position)
        {
            try
            {
                output[position] = (byte)value;
                return 1;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BufferTooSmallException();
            }
        }

        /// <summary>
        /// Decodes an 8-bit signed integer.
        /// </summary>
        /// <param name="source">The source array for this data</param>
        /// <param name="position">The position in <paramref name="source"/> to decode from.</param>
        /// <returns>The decoded value.</returns>
        public static sbyte decodeInt8(byte[] source, int position)
        {
            try
            {
                return (sbyte)source[position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new BFlatException("invalid int8 value", position);
            }
        }

        /// <summary>
        /// Encodes a 16-bit signed integer.
        /// </summary>
        /// <param name="value">The value to encode</param>
        /// <param name="output">The destination array</param>
        /// <param name="position">The posiiton in <paramref name="output"/> to write to.</param>
        /// <returns>The number of bytes written to <paramref name="output"/></returns>
        public static int encodeInt16(short value, byte[] output, int position)
        {
            try
            {
                output[position] = (byte)(value & 0xff);
                output[position + 1] = (byte)((value & 0xff00) >> 8);
                return 2;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BufferTooSmallException("while encoding int16");
            }
        }

        /// <summary>
        /// Decodes a 16-bit signed integer.
        /// </summary>
        /// <param name="source">The source array for this data</param>
        /// <param name="position">The position in <paramref name="source"/> to decode from.</param>
        /// <returns>The decoded value.</returns>
        public static short decodeInt16(byte[] source, int position)
        {
            try
            {
                int value = 0;
                for (int i = 0; i < 2; ++i)
                {
                    short mask = 0xff;
                    short val = (short)(source[position + i] & mask);
                    value |= (val << (i * 8));
                }
                return (short)value;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BFlatException("invalid int16 value", position);
            }
        }

        /// <summary>
        /// Encodes a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The value to encode</param>
        /// <param name="output">The destination array</param>
        /// <param name="position">The posiiton in <paramref name="output"/> to write to.</param>
        /// <returns>The number of bytes written to <paramref name="output"/></returns>
        public static int encodeInt32(int value, byte[] output, int position)
        {
            try
            {
                output[position] = (byte)(value & 0xff);
                output[position + 1] = (byte)((value & 0xff00) >> 8);
                output[position + 2] = (byte)((value & 0xff0000) >> 16);
                output[position + 3] = (byte)((value & 0xff000000) >> 24);
                return 4;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BufferTooSmallException("while encoding int32");
            }
        }

        /// <summary>
        /// Decodes a 32-bit signed integer.
        /// </summary>
        /// <param name="source">The source array for this data</param>
        /// <param name="position">The position in <paramref name="source"/> to decode from.</param>
        /// <returns>The decoded value.</returns>
        public static int decodeInt32(byte[] source, int position)
        {
            try
            {
                int value = 0;
                for (int i = 0; i < 4; ++i)
                {
                    int val = source[position + i] & 0xff;
                    value |= val << (i * 8);
                }
                return value;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BFlatException("invalid int32 value", position);
            }
        }

        /// <summary>
        /// Encodes a 64-bit signed integer.
        /// </summary>
        /// <param name="value">The value to encode</param>
        /// <param name="output">The destination array</param>
        /// <param name="position">The posiiton in <paramref name="output"/> to write to.</param>
        /// <returns>The number of bytes written to <paramref name="output"/></returns>
        public static int encodeInt64(long value, byte[] output, int position)
        {
            try
            {
                output[position] = (byte)(value & 0xff);
                output[position + 1] = (byte)((value & 0xff00) >> 8);
                output[position + 2] = (byte)((value & 0xff0000) >> 16);
                output[position + 3] = (byte)((value & 0xff000000) >> 24);
                output[position + 4] = (byte)((value & 0xff00000000L) >> 32);
                output[position + 5] = (byte)((value & 0xff0000000000L) >> 40);
                output[position + 6] = (byte)((value & 0xff000000000000L) >> 48);
                output[position + 7] = (byte)((value & unchecked((long)0xff00000000000000L)) >> 56);
                return 8;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BufferTooSmallException("while encoding int64");
            }
        }

        /// <summary>
        /// Decodes a 64-bit signed integer.
        /// </summary>
        /// <param name="source">The source array for this data</param>
        /// <param name="position">The position in <paramref name="source"/> to decode from.</param>
        /// <returns>The decoded value.</returns>
        public static long decodeInt64(byte[] source, int position)
        {
            try
            {
                long value = 0;
                for (int i = 0; i < 8; ++i)
                {
                    long val = source[position + i] & 0xff;
                    value |= val << (i * 8);
                }
                return value;
            }
            catch (IndexOutOfRangeException)
            {
                throw new BFlatException("invalid int64 value", position);
            }
        }

        /// <summary>
        /// Encodes an IEEE-754 double precision value.
        /// </summary>
        /// <param name="value">The value to encode</param>
        /// <param name="output">The destination array</param>
        /// <param name="position">The posiiton in <paramref name="output"/> to write to.</param>
        /// <returns>The number of bytes written to <paramref name="output"/></returns>
        public static int encodeDouble(double value, byte[] output, int position)
        {
            long dval = BitConverter.DoubleToInt64Bits(value);
            return encodeInt64(dval, output, position);
        }

        /// <summary>
        /// Decodes a IEEE-754 double precision value.
        /// </summary>
        /// <param name="source">The source array for this data</param>
        /// <param name="position">The position in <paramref name="source"/> to decode from.</param>
        /// <returns>The decoded value.</returns>
        public static double decodeDouble(byte[] source, int position)
        {
            long dval = decodeInt64(source, position);
            return BitConverter.Int64BitsToDouble(dval);
        }

    }
}
