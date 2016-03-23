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
    /// BFlatValue represents a scalar value or an array in a parsed BFlat
    /// message. BFlatValue objects are returned by <see cref="BFlatParser"/>
    /// when iterating over a BFlat document.
    ///
    /// For best performance:
    /// <list type="bullet">
    /// <item><description>Use the specific methods <see cref="getInt8"/>,
    /// <see cref="getInt16"/>, <see cref="getInt32"/>, <see cref="getInt64"/>,
    /// <see cref="getDatetime"/> and <see cref="getLeb128"/> instead of
    /// <see cref="getLong"/> when a specific data type is expected.
    /// </description></item>
    /// <item><description>While parsing a document. invoke the
    /// <see cref="reuse"/> method once you are finished using the BFlatValue,
    /// so that BFlatParser can re-use it and reduce the number of objects
    /// created.</description></item>
    /// <item><description>If you do not need a String version of the value's
    /// tag or data, use <see cref="getRawTagOffset"/>,
    /// <see cref="getRawTagLength"/>, <see cref="getRawValueOffset"/> and
    /// <see cref="getRawValueLength"/> to locate the tag and string data in
    /// the source array without making temporary objects or allocating new
    /// objects.</description></item>
    /// </list>
    /// </summary>
    public sealed class BFlatValue
    {

        /// <summary>
        /// Mark this object as reusable. Calling this before invoking
        /// <see cref="BFlatParser.MoveNext"/> or before allowing your foreach
        /// to loop will allow the parser to re-use this object and reduce the
        /// number of temporary objects created.
        /// </summary>
        public void reuse()
        {
            _isReuse = true;
        }


        /// <summary>
        /// The buffer underlying this value. This is typically the same buffer
        /// as is passed to the <see cref="BFlatValue(byte[])"/> constructor.
        /// Notice this value does not necessarily begin at the first byte of
        /// the underlying buffer.
        /// </summary>
        /// <returns>The underlying buffer.</returns>
        public byte[] getBuffer()
        {
            return _data;
        }

        /// <summary>
        /// Returns a string reprensentation of this tag/value (or tag/array)
        /// pair.
        /// </summary>
        /// <returns>A string representation of self.</returns>
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Encoding.UTF8.GetString(_data, _tagOffset, _tagLength));
            sb.Append("=");
            bool isArray = this.isArray();
            if (isArray)
            {
                sb.Append("[");
            }
            for (int i = 0; i < _count; ++i)
            {
                if (i > 0) sb.Append(", ");
                switch (getType())
                {
                    case BFlatEncoding.Type.String:
                    case BFlatEncoding.Type.Binary:
                        sb.Append("\"");
                        sb.Append(getString(i));
                        sb.Append("\"");
                        break;
                    case BFlatEncoding.Type.Int8:
                    case BFlatEncoding.Type.Int16:
                    case BFlatEncoding.Type.Int32:
                    case BFlatEncoding.Type.Int64:
                    case BFlatEncoding.Type.Leb128:
                    case BFlatEncoding.Type.Datetime:
                        sb.Append(getLong(i));
                        break;
                    case BFlatEncoding.Type.Null:
                        sb.Append("null");
                        break;
                    case BFlatEncoding.Type.Double:
                        sb.Append(getDouble(i));
                        break;
                }
            }
            if (isArray)
            {
                sb.Append("]");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns the data type of this BFlatValue.
        /// </summary>
        /// <returns>One of the constants defined in
        ///   <see cref="BFlatEncoding.Type"/></returns>
        public BFlatEncoding.Type getType()
        {
            return (BFlatEncoding.Type)(_type & BFlatEncoding.TypeMask);
        }

        /// <summary>
        /// Returns the index in the underlying buffer where this value's tag
        /// begins. Use this method along with <see cref="getRawTagLength"/> to
        /// extract the tag name without the performance penalty of allocating
        /// a new string or transcoding.
        /// </summary>
        /// <returns>The index into this value's underlying buffer where
        ///   the tag begins.</returns>
        public int getRawTagOffset()
        {
            return _tagOffset;
        }

        /// <summary>
        /// Returns the length, in bytes, of the tag name. Use this method
        /// along with <see cref="getRawTagOffset"/> to extract the tag name
        /// without the performance penalty of allocating a new string or
        /// transcoding.
        /// </summary>
        /// <returns>The length of this tag (in bytes) in the underlying
        /// buffer.</returns>
        public int getRawTagLength()
        {
            return _tagLength;
        }

        /// <summary>
        /// Returns this value's tag as a string.
        /// </summary>
        /// <returns>This value's tag name, converted to a string.</returns>
        public String getTag()
        {
            return Encoding.UTF8.GetString(_data, getRawTagOffset(),
                                           getRawTagLength());
        }

        /// <summary>
        /// Returns true if this value is an array. A single BFlatValue object
        /// represents a scalar or all of the values in an array.
        /// </summary>
        /// <returns>true if this value is an array, false otherwose.</returns>
        public bool isArray()
        {
            return (_type & BFlatEncoding.ArrayMask) != 0;
        }

        /// <summary>
        /// If this value is an array, returns the number of elements in this
        /// array. The return value of this method is undefined if this value
        /// is not an array.
        /// </summary>
        /// <returns>The count of elements in this array.</returns>
        public int getArrayLength()
        {
            return _count;
        }

        /// <summary>
        /// Returns true if this value is Null. This method is equivalent to
        /// comparing <see cref="getType()"/> to
        /// <see cref="BFlatEncoding.Type.Null"/>.
        /// </summary>
        /// <returns>true if this value is null, false otherwise.</returns>
        public bool isNull()
        {
            return getType() == BFlatEncoding.Type.Null;
        }

        /// <summary>
        /// Returns the 8-bit signed integer value of self. The result of this
        /// method is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.Int8"/>
        /// </summary>
        /// <returns>the value of self.</returns>
        public sbyte getInt8()
        {
            return Fixed.decodeInt8(_data, _dataOffset);
        }

        /// <summary>
        /// Returns the 8-bit signed integer value of an array element of self.
        /// The result of this method is only defined if <see cref="getType"/>
        /// is <see cref="BFlatEncoding.Type.Int8"/> and
        /// <paramref name="arrayIndex"/> is less than the return value of
        /// <see cref="getArrayLength"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index into this array.</param>
        /// <returns>The value of this array element.</returns>
        public sbyte getInt8(int arrayIndex)
        {
            return Fixed.decodeInt8(_data, _dataOffset + arrayIndex);
        }

        /// <summary>
        /// Returns the 16-bit signed integer value of self. The result of
        /// this method is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.Int16"/>
        /// </summary>
        /// <returns>the value of self.</returns>
        public short getInt16()
        {
            return Fixed.decodeInt16(_data, _dataOffset);
        }
        /// <summary>
        /// Returns the 16-bit signed integer value of an array element of
        /// self. The result of this method is only defined if
        /// <see cref="getType"/> is <see cref="BFlatEncoding.Type.Int16"/>
        /// and <paramref name="arrayIndex"/> is less than the return value of
        /// <see cref="getArrayLength"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index into this array.</param>
        /// <returns>The value of this array element.</returns>
        public short getInt16(int arrayIndex)
        {
            return Fixed.decodeInt16(_data, _dataOffset + (2 * arrayIndex));
        }

        /// <summary>
        /// Returns the 32-bit signed integer value of self. The result of
        /// this method is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.Int32"/>
        /// </summary>
        /// <returns>the value of self.</returns>
        public int getInt32()
        {
            return Fixed.decodeInt32(_data, _dataOffset);
        }
        /// <summary>
        /// Returns the 32-bit signed integer value of an array element of
        /// self. The result of this method is only defined if
        /// <see cref="getType"/> is <see cref="BFlatEncoding.Type.Int32"/>
        /// and <paramref name="arrayIndex"/> is less than the return value of
        /// <see cref="getArrayLength"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index into this array.</param>
        /// <returns>The value of this array element.</returns>
        public int getInt32(int arrayIndex)
        {
            return Fixed.decodeInt32(_data, _dataOffset + (4 * arrayIndex));
        }

        /// <summary>
        /// Returns the 64-bit signed integer value of self. The result of
        /// this method is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.Int64"/>
        /// </summary>
        /// <returns>the value of self.</returns>
        public long getInt64()
        {
            return Fixed.decodeInt64(_data, _dataOffset);
        }
        /// <summary>
        /// Returns the 64-bit signed integer value of an array element of
        /// self. The result of this method is only defined if
        /// <see cref="getType"/> is <see cref="BFlatEncoding.Type.Int64"/>,
        /// and <paramref name="arrayIndex"/> is less than the return value of
        /// <see cref="getArrayLength"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index into this array.</param>
        /// <returns>The value of this array element.</returns>
        public long getInt64(int arrayIndex)
        {
            return Fixed.decodeInt64(_data, _dataOffset + (8 * arrayIndex));
        }

        /// <summary>
        /// Returns the 64-bit signed integer value of self. The result of
        /// this method is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.Datetime"/>
        /// </summary>
        /// <returns>the value of self.</returns>
        public long getDatetime()
        {
            return Fixed.decodeInt64(_data, _dataOffset);
        }
        /// <summary>
        /// Returns the 64-bit signed integer value of an array element of
        /// self. The result of this method is only defined if
        /// <see cref="getType"/> is <see cref="BFlatEncoding.Type.Datetime"/>,
        /// and <paramref name="arrayIndex"/> is less than the return value of
        /// <see cref="getArrayLength"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index into this array.</param>
        /// <returns>The value of this array element.</returns>
        public long getDatetime(int arrayIndex)
        {
            return Fixed.decodeInt64(_data, _dataOffset + (8 * arrayIndex));
        }

        /// <summary>
        /// Returns the double-precision value of self. The result of
        /// this method is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.Double"/>
        /// </summary>
        /// <returns>the value of self.</returns>
        public double getDouble()
        {
            return Fixed.decodeDouble(_data, _dataOffset);
        }
        /// <summary>
        /// Returns the double-precision value of an array element of
        /// self. The result of this method is only defined if
        /// <see cref="getType"/> is <see cref="BFlatEncoding.Type.Double"/>,
        /// and <paramref name="arrayIndex"/> is less than the return value of
        /// <see cref="getArrayLength"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index into this array.</param>
        /// <returns>The value of this array element.</returns>
        public double getDouble(int arrayIndex)
        {
            return Fixed.decodeDouble(_data, _dataOffset + (8 * arrayIndex));
        }

        /// <summary>
        /// Returns the offset into this value's underlying buffer where
        /// this value's string or binary data begins. The result of this
        /// method is only defined if <see cref="getType"/> is <see
        /// cref="BFlatEncoding.Type.String"/> or
        /// <see cref="BFlatEncoding.Type.Binary"/>.
        /// </summary>
        /// <returns>The offset into the underlying buffer where the data
        ///   for this value begins.</returns>
        public int getRawValueOffset()
        {
            return (int)(_hint >> 32);
        }
        /// <summary>
        /// Returns the length of this value's data, in bytes.
        /// The result of this method is only defined if <see cref="getType"/>
        /// is <see cref="BFlatEncoding.Type.String"/> or
        /// <see cref="BFlatEncoding.Type.Binary"/>.
        /// </summary>
        /// <returns>The length of this value's data, in bytes.</returns>
        public int getRawValueLength()
        {
            return (int)(_hint & 0xFFFFFFFF);
        }

        /// <summary>
        /// Returns this value's string data. The result of this method is
        /// only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.String"/> or
        /// <see cref="BFlatEncoding.Type.Binary"/>.
        /// </summary>
        /// <returns>This value's string data.</returns>
        public String getString()
        {
            return Encoding.UTF8.GetString(_data, getRawValueOffset(),
                                           getRawValueLength());
        }

        /// <summary>
        /// Returns the offset into this value's underlying buffer where
        /// the specified array element begins. The result of this method
        /// is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.String"/>
        /// or
        /// <see cref="BFlatEncoding.Type.Binary"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index of the string or
        ///   binary element of self.</param>
        /// <returns>The offset where the specified element begins.</returns>
        public int getRawValueOffset(int arrayIndex)
        {
            if (arrayIndex == 0) return getRawValueOffset();
            return (int)((_arrayHints[arrayIndex - 1]) >> 32);
        }

        /// <summary>
        /// Returns the length of the specified array element data in bytes.
        /// The result of this method
        /// is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.String"/>
        /// or
        /// <see cref="BFlatEncoding.Type.Binary"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index of the string or
        ///   binary element of self.</param>
        /// <returns>The length of the raw value of the specified element.
        ///   </returns>
        public int getRawValueLength(int arrayIndex)
        {
            if (arrayIndex == 0) return getRawValueLength();
            return (int)((_arrayHints[arrayIndex - 1]) & 0xFFFFFFFF);
        }
        /// <summary>
        /// Returns the string value of the specified array element.
        /// The result of this method
        /// is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.String"/>
        /// or
        /// <see cref="BFlatEncoding.Type.Binary"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index of the string element
        ///   of self.</param>
        /// <returns>The string value of self.</returns>
        public String getString(int arrayIndex)
        {
            long v = _hint;
            if (arrayIndex > 0) v = _arrayHints[arrayIndex - 1];
            return Encoding.UTF8.GetString(_data, (int)(v >> 32),
                                           (int)(v & 0xFFFFFFFF));
        }

        /// <summary>
        /// Returns the signed integer value of self. The result of this
        /// method is only defined if <see cref="getType"/> is
        /// <see cref="BFlatEncoding.Type.Leb128"/>.
        /// </summary>
        /// <returns>The signed integer equivalent of this value.</returns>
        public long getLeb128()
        {
            return _hint;
        }

        /// <summary>
        /// Returns the signed integer value of an array element of self.
        /// The result of this method is only defined if
        /// <see cref="getType"/> is <see cref="BFlatEncoding.Type.Leb128"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index of the LEB128-encoded
        ///  element of self.</param>
        /// <returns>The signed integer value of the specified element.
        /// </returns>
        public long getLeb128(int arrayIndex)
        {
            if (arrayIndex == 0) return _hint;
            return _arrayHints[arrayIndex - 1];
        }

        /// <summary>
        /// Returns the signed integer value of self. This method may be
        /// used for any value whose <see cref="getType"/> returns
        /// <see cref="BFlatEncoding.Type.Int8"/>,
        /// <see cref="BFlatEncoding.Type.Int16"/>,
        /// <see cref="BFlatEncoding.Type.Int32"/>,
        /// <see cref="BFlatEncoding.Type.Int64"/>,
        /// <see cref="BFlatEncoding.Type.Datetime"/>, or
        /// <see cref="BFlatEncoding.Type.Leb128"/>.
        /// </summary>
        /// <returns>The long value of self.</returns>
        public long getLong()
        {
            switch (getType())
            {
                case BFlatEncoding.Type.Int8:
                    return getInt8();
                case BFlatEncoding.Type.Int16:
                    return getInt16();
                case BFlatEncoding.Type.Int32:
                    return getInt32();
                case BFlatEncoding.Type.Int64:
                    return getInt64();
                case BFlatEncoding.Type.Datetime:
                    return getDatetime();
                case BFlatEncoding.Type.Leb128:
                    return getLeb128();
                case BFlatEncoding.Type.Null:
                    throw new BFlatException("value is null");
                default:
                    throw new BFlatException("cannot convert to integer");
            }
        }
        /// <summary>
        /// Returns the signed integer value of self. This method may be
        /// used for any value whose <see cref="getType"/> returns
        /// <see cref="BFlatEncoding.Type.Int8"/>,
        /// <see cref="BFlatEncoding.Type.Int16"/>,
        /// <see cref="BFlatEncoding.Type.Int32"/>,
        /// <see cref="BFlatEncoding.Type.Int64"/>,
        /// <see cref="BFlatEncoding.Type.Datetime"/>, or
        /// <see cref="BFlatEncoding.Type.Leb128"/>.
        /// </summary>
        /// <param name="arrayIndex">The 0-based index of an array element
        ///   of self.</param>
        /// <returns>The long value of self.</returns>
        public long getLong(int arrayIndex)
        {
            switch (getType())
            {
                case BFlatEncoding.Type.Int8:
                    return getInt8(arrayIndex);
                case BFlatEncoding.Type.Int16:
                    return getInt16(arrayIndex);
                case BFlatEncoding.Type.Int32:
                    return getInt32(arrayIndex);
                case BFlatEncoding.Type.Int64:
                    return getInt64(arrayIndex);
                case BFlatEncoding.Type.Datetime:
                    return getDatetime(arrayIndex);
                case BFlatEncoding.Type.Leb128:
                    return getLeb128(arrayIndex);
                case BFlatEncoding.Type.Null:
                    throw new BFlatException("value is null");
                default:
                    throw new BFlatException("cannot convert to integer");
            }
        }

        internal BFlatValue(byte[] data)
        {
            _data = data;
        }

        internal bool isReuse()
        {
            return _isReuse;
        }

        internal BFlatValue reset(byte[] data)
        {
            _data = data;
            _isReuse = false;
            return this;
        }

        internal void setBuffer(byte[] data)
        {
            _data = data;
        }

        internal void setTag(int offset, int len)
        {
            _tagOffset = offset;
            _tagLength = len;
        }

        internal void setData(int position, byte type, int count)
        {
            _dataOffset = position;
            _type = type;
            _count = count;
            if (_count > 1 && (getType() == BFlatEncoding.Type.String || getType() == BFlatEncoding.Type.Binary ||
                getType() == BFlatEncoding.Type.Leb128))
            {
                if (_arrayHints == null || _arrayHints.Length < _count - 1)
                {
                    _arrayHints = new long[_count - 1];
                }
            }
        }

        internal void setStringOffsetAndLen(int pos, int offset, int length)
        {
            long longoffset = offset;

            long val = longoffset << 32;
            val |= (uint)length;
            if (pos == 0)
                _hint = val;
            else
                _arrayHints[pos - 1] = val;
        }

        internal void setLeb128Value(int pos, long value)
        {
            if (pos == 0)
                _hint = value;
            else
                _arrayHints[pos - 1] = value;
        }

        byte[] _data;
        long[] _arrayHints;
        long _hint;
        int _tagOffset;
        int _tagLength;
        int _dataOffset;
        int _count;
        byte _type;
        bool _isReuse;
    }
}
