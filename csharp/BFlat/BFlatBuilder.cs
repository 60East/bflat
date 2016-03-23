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
using System.Text;

namespace BFlat
{
    /// <summary>
    /// BFlatBuilder is a class used to encode values into a byte array of
    /// BFlat format. Construct BFlatBuilder with the byte array to encode
    /// values into, and then use an "encode" method to encode tag and value
    /// pairs into the byte array. To encode arrays use "encodeTagArray"
    /// to supply the type and number of elements, followed by "encode" to
    /// encode the elements of the array.
    /// 
    /// Example of encoding scalar tag/value pairs into BFlat format:
    /// 
    /// <code>
    /// // Allocate an array
    /// byte[] outputBuffer = new byte[1024];
    /// BFlatBuilder builder = new BFlatBuilder(outputBuffer, 0);
    /// // Encode { "tag_1": 10, "tag_2": "value"}
    /// builder.encode("tag_1",10);
    /// builder.encode("tag_2","value");
    /// 
    /// // The current number of bytes encoded is in builder.position.
    /// int finalLength = builder.position;
    /// </code>
    /// 
    /// Example of encoding an array into BFlat format:
    /// <code>
    /// // Allocate an output array
    /// byte[] outputBuffer = new byte[1024];
    /// BFlatBuilder builder = new BFlatBuilder(outputBuffer, 0);
    /// // Encode { "array_1",[0,1,-1,32]}
    /// builder.encodeTagArray(BFlatEncoding.Int32, "array_1", 4);
    /// builder.encode(0).encode(1).encode(-1).encode(32);
    /// </code>
    /// 
    /// </summary>
    public class BFlatBuilder : BFlat.Buffer
    {

        /// <summary>
        /// Construct BFlatBuilder with an existing byte array to write into.
        /// </summary>
        /// <param name="data">The output byte array for this builder.</param>
        /// <param name="position">The index of <paramref name="data"/>
        ///  to begin writing at.</param>
        public BFlatBuilder(byte[] data, int position)
        {
            this.data = data;
            this.position = position;
            this.start = this.position;
        }

        /// <summary>
        /// Encode a tag into this byte array. A call to encodeTag must be
        /// followed by a call to "encode" unless type <paramref name="type"/>
        /// is <see cref="BFlatEncoding.Type.Null"/>.
        /// </summary>
        /// <param name="type">A value from <see cref="BFlatEncoding.Type"/>
        ///                    </param>
        /// <param name="tagName">The UTF-8 encoded tag name.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encodeTag(BFlatEncoding.Type type, byte[] tagName)
        {
            return encodeTag((byte)type, tagName);
        }

        /// <summary>
        /// Encode a tag into this byte array. A call to encodeTag must be
        /// followed by a call to "encode" unless type <paramref name="type"/>
        /// is <see cref="BFlatEncoding.Type.Null"/>.
        /// </summary>
        /// <param name="type">A value from <see cref="BFlatEncoding.Type"/>
        ///                    </param>
        /// <param name="tagName">The tag name.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encodeTag(BFlatEncoding.Type type, string tagName)
        {
            return encodeTag((byte)type, tagName);
        }

        // Internal version of encodeTag takes a byte as the first parameter.
        internal BFlatBuilder encodeTag(byte type, byte[] tagName)
        {
            if (tagName.Length == 0)
            {
                throw new BFlatException("Zero length tags are not allowed.");
            }
            int tagLen = tagName.Length;
            if (tagLen < 8)
            {
                // fit the tag len into the byte0
                data[position++] = (byte)((int)type | tagLen);
                Array.Copy(tagName, 0, data, position, tagLen);
                position += tagLen;
            }
            else
            {
                data[position++] = (byte)type;
                Leb128.encodeUnsigned(this, (uint)tagLen);
                Array.Copy(tagName, 0, data, position, tagLen);
                position += tagLen;
            }
            return this;
        }

        // Internal version just uses a "byte" as the first parameter.
        internal BFlatBuilder encodeTag(byte type, String tagName)
        {
            if (tagName.Length == 0)
            {
                throw new BFlatException("Zero length tags are not allowed.");
            }
            uint tagLen = (uint)tagName.Length;
            if (tagLen < 8)
            {
                // see if fit the tag len into the byte0
                int byte0 = position++;
                uint actual = encodeString(tagName);
                if (actual < 8)
                {
                    data[byte0] = (byte)(type | actual);
                }
                else
                {
                    // since the tag <8 characters we know that the # of bytes
                    // it will fit in is representable with a one byte leb128
                    data[byte0] = (byte)type;
                    Array.Copy(data, byte0 + 1, data, byte0 + 2, actual);
                    data[byte0 + 1] = (byte)actual;
                    ++position;
                }
            }
            else
            {
                data[position++] = (byte)type;
                int start = position;
                int leblength = Leb128.encodeUnsigned(this, tagLen);
                uint actual = encodeString(tagName);
                if (actual != tagLen)
                {
                    int end = position;
                    position = start;
                    int actual_leblength = Leb128.encodeUnsigned(this, actual);
                    // do we need to reserialize the data?
                    if (actual_leblength != leblength)
                    {
                        encodeString(tagName);
                    }
                    else
                    {
                        position = end;
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Encode an array tag into this byte array. A call to this method
        /// must be followed by <paramref name="count"/> calls to "encode".
        /// </summary>
        /// <param name="type">One of the type codes from
        ///   <see cref="BFlatEncoding.Type"/>.</param>
        /// <param name="tagName">The UTF-8 encoded tag name.</param>
        /// <param name="count">The count of elements in this array.
        ///   The number of calls to "encode" must match this value,
        ///   or a malformed BFlat document will be produced.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder
        encodeTagArray(BFlatEncoding.Type type, byte[] tagName, int count)
        {
            encodeTag((byte)((byte)type | BFlatEncoding.ArrayMask), tagName);
            Leb128.encodeUnsigned(this, (uint)count);
            return this;
        }

        /// <summary>
        /// Encode an array tag into this byte array. A call to this method
        /// must be followed by <paramref name="count"/> calls to "encode".
        /// </summary>
        /// <param name="type">One of the type codes from
        ///   <see cref="BFlatEncoding.Type"/>.</param>
        /// <param name="tagName">The tag name.</param>
        /// <param name="count">The count of elements in this array.
        ///   The number of calls to "encode" must match this value,
        ///   or a malformed BFlat document will be produced.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder
        encodeTagArray(BFlatEncoding.Type type, String tagName, int count)
        {
            encodeTag((byte)((byte)type | BFlatEncoding.ArrayMask), tagName);
            Leb128.encodeUnsigned(this, (uint)count);
            return this;
        }

        /// <summary>
        /// Copy a previously-encoded UTF-8 string value into this byte array.
        /// A call to this method must be preceded by a call to "encodeTag"
        /// or "encodeTagArray". The caller must ensure the byte array is a
        /// valid UTF-8 encoded string; no checking is performed.
        /// </summary>
        /// <param name="value">The byte array containing a UTF-8 string
        ///                     to be included in the output message.</param>
        /// <param name="offset">The offset in <paramref name="offset"/>
        ///                     where the encoded UTF-8 string begins.</param>
        /// <param name="length">The length of the UTF-8 encoded stirng,
        ///   in bytes, to be written to the output message.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(byte[] value, int offset, int length)
        {
            Leb128.encodeUnsigned(this, (uint)length);
            Array.Copy(value, offset, data, position, length);
            position += length;
            return this;
        }

        /// <summary>
        /// Encode a string value into this byte array. A call to this method
        /// must be preceded by a call to "encodeTag" or "encodeTagArray."
        /// </summary>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(String value)
        {
            int charcount = value.Length;
            int start = position;
            int leblength = Leb128.encodeUnsigned(this, (uint)charcount);
            uint bytecount = encodeString(value);
            int end = position;
            if (bytecount != charcount) // non ascii
            {
                position = start;
                int actual_leblength = Leb128.encodeUnsigned(this, bytecount);
                if (actual_leblength != leblength)
                {
                    //reserialize the string
                    encodeString(value);
                }
                else
                {
                    position = end;
                }
            }
            return this;
        }
        /// <summary>
        /// Encode a byte value into this byte array. A call to this method
        /// must be preceded by a call to "encodeTag" or "encodeTagArray."
        /// </summary>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(sbyte value)
        {
            position += Fixed.encodeInt8(value, data, position);
            return this;
        }
        /// <summary>
        /// Encode a short value into this byte array. A call to this method
        /// must be preceded by a call to "encodeTag" or "encodeTagArray."
        /// </summary>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(short value)
        {
            position += Fixed.encodeInt16(value, data, position);
            return this;
        }
        /// <summary>
        /// Encode an integer value into this byte array. A call to this method
        /// must be preceded by a call to "encodeTag" or "encodeTagArray."
        /// </summary>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(int value)
        {
            position += Fixed.encodeInt32(value, data, position);
            return this;
        }
        /// <summary>
        /// Encode a long value into this byte array. A call to this method
        /// must be preceded by a call to "encodeTag" or "encodeTagArray."
        /// </summary>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(long value)
        {
            position += Fixed.encodeInt64(value, data, position);
            return this;
        }
        /// <summary>
        /// Encode a double value into this byte array. A call to this method
        /// must be preceded by a call to "encodeTag" or "encodeTagArray."
        /// </summary>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(double value)
        {
            position += Fixed.encodeDouble(value, data, position);
            return this;
        }

        /// <summary>
        /// Encode a tag and string value into this byte array.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(String tagName, String value)
        {
            encodeTag((byte)BFlatEncoding.Type.String, tagName);
            encode(value);
            return this;
        }

        /// <summary>
        /// Encode a tag and sbyte value into this byte array.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(String tagName, sbyte value)
        {
            encodeTag((byte)BFlatEncoding.Type.Int8, tagName);
            encode(value);
            return this;
        }

        /// <summary>
        /// Encode a tag and short value into this byte array.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(String tagName, short value)
        {
            encodeTag((byte)BFlatEncoding.Type.Int16, tagName);
            encode(value);
            return this;
        }

        /// <summary>
        /// Encode a tag and int value into this byte array.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(String tagName, int value)
        {
            encodeTag((byte)BFlatEncoding.Type.Int32, tagName);
            encode(value);
            return this;
        }

        /// <summary>
        /// Encode a tag and long value into this byte array.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(String tagName, long value)
        {
            encodeTag((byte)BFlatEncoding.Type.Int64, tagName);
            encode(value);
            return this;
        }

        /// <summary>
        /// Encode a tag and double value into this byte array.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="value">The value to encode into this array.</param>
        /// <returns>This builder.</returns>
        public BFlatBuilder encode(String tagName, double value)
        {
            encodeTag((byte)(BFlatEncoding.Type.Double), tagName);
            encode(value);
            return this;
        }

        //
        // This utility method performs a fast UTF-16 to UTF-8 conversion 
        // manually but falls back to the system when we have surrogates to
        // convert.
        //
        private uint encodeString(String value)
        {
            int length = value.Length;
            int i = 0;
            int start = position;
            for (; i < length; ++i)
            {
                char c = value[i];
                if (c < 128) data[position++] = (byte)c;
                else break;
            }
            while (i < length)
            {
                char c = value[i++];
                if (c <= 127) data[position++] = (byte)c;
                else if (c <= 0x7ff)
                {
                    data[position++] = (byte)(0xc0 | (c >> 6));
                    data[position++] = (byte)(0x80 | (c & 0x3f));
                }
                else if (Char.IsSurrogate(c))
                {
                    // fall back to platform version of this
                    position = start;
                    byte[] bytes = Encoding.UTF8.GetBytes(value);
                    Array.Copy(bytes, 0, data, position, bytes.Length);
                    position += length;
                    return (uint)length;
                }
                else
                {
                    data[position++] = (byte)(0xe0 | (c >> 12));
                    data[position++] = (byte)(0x80 | ((c >> 6) & 0x3f));
                    data[position++] = (byte)(0x80 | (c & 0x3f));
                }
            }
            return (uint)(position - start);
        }
    }
}
