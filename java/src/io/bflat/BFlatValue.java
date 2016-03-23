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
package io.bflat;
import java.nio.charset.Charset;

/**
 * BFlatValue represents a scalar value or an array in a parsed BFlat message.
 * BFlatValue objects are returned by {@link BFlatParser} when iterating over
 * a BFlat document.
 * <p>
 * For best performance:
 * <ul>
 * <li>Use the specific methods {@link #getInt8}, {@link #getInt16},
 *     {@link #getInt32}, {@link #getInt64}, {@link #getDatetime} and
 *     {@link #getLeb128} instead of {@link #getLong} when a specific data
 *     type is expected.
 * <li>While parsing a document, invoke the {@link #reuse} method once
 *     you are finished using the BFlatValue, so that {@link BFlatParser}
 *     can re-use it and reduce the number of objects created.
 * <li>If you do not need a String version of the value's tag or data,
 *     use {@link #getRawTagOffset}, {@link #getRawTagLength},
 *     {@link #getRawValueOffset} and {@link #getRawValueLength} to locate the
 *     tag and string data in the source array without making temporary copies
 *     or allocating new objects.
 * </ul>
 *
 */
public final class BFlatValue
{
  /**
   * Mark this object as reusable. Calling this before invoking
   * {@link BFlatParser#next} or before allowing a range-for to loop will allow
   * the parser to re-use this object and reduce the number of temporary
   * objects created.
   */
  public void reuse()
  {
    _isReuse=true;
  }

  /**
   * The buffer underlying this value. This is typically the same buffer
   * as is passed to the {@link BFlatValue} <tt>parse</tt> method. Notice that
   * this value does not necessarily begin at the first byte of the underlying
   * buffer. 
   * @return the underlying buffer
   */
  public byte[] getBuffer()
  {
    return _data;
  }

  /**
   * Returns a String representation of the tag and value or array.
   * @return A string of the format <tt>tag=value</tt> or <tt>array=[1,2,3]</tt>
   */
  public String toString()
  {
    StringBuilder sb = new StringBuilder();
    sb.append(new String(_data,_tagOffset,_tagLength,
          UTF_8));
    sb.append("=");
    boolean isArray = isArray();
    if(isArray)
    {
      sb.append("[");
    }
    for(int i =0; i < _count; ++i)
    {
      if(i>0) sb.append(", ");
      switch(getType())
      {
        case BFlatEncoding.String:
        case BFlatEncoding.Binary:
          sb.append("\"");
          sb.append(getString(i));
          sb.append("\"");
          break;
        case BFlatEncoding.Int8:
        case BFlatEncoding.Int16:
        case BFlatEncoding.Int32:
        case BFlatEncoding.Int64:
        case BFlatEncoding.Leb128:
        case BFlatEncoding.Datetime:
          sb.append(getLong(i));
          break;
        case BFlatEncoding.Null:
          sb.append("null");
          break;
        case BFlatEncoding.Double:
          sb.append(getDouble(i));
      }
    }
    if(isArray)
    {
      sb.append("]");
    }
    return sb.toString();
  }

  /**
   * Returns the data type of this BFlatValue.
   * @return One of the constants defined in {@link BFlatEncoding}.
   */
  public byte getType()
  {
    return (byte)(_type & BFlatEncoding.TypeMask);
  }

  /**
   * Returns the index in the underlying buffer where this value's tag begins.
   * Use this method along with {@link #getRawTagLength} to extract the tag name
   * without the performance penalty of allocating a new {@link String} or
   * transcoding.
   * @return The index into this value's underlying buffer where the tag begins.
   */
  public int getRawTagOffset()
  {
    return _tagOffset;
  }
  /**
   * Returns the length, in bytes, of the tag name. Use this method along with
   * {@link #getRawTagOffset} to extract the tag name without the performance
   * penalty of allocating a new {@link String} or transcoding.
   * @return The length of this tag in the underlying buffer.
   */
  public int getRawTagLength()
  {
    return _tagLength;
  }
  /**
   * Returns this value's tag.
   * @return This value's tag name.
   */
  public String getTag()
  {
    return new String(_data,getRawTagOffset(),getRawTagLength(),UTF_8);
  }
  /**
   * Returns true if this value is an array. A single BFlatValue object
   * represents a scalar or all of the values in an array.
   * @return true if this value is an array, false otherwise.
   */
  public boolean isArray()
  {
    return (_type & BFlatEncoding.ArrayMask) != 0;
  }
  /**
   * If this value is an array, returns the number of elements in this array.
   * @return the number of elements in this array.
   */
  public int getArrayLength()
  {
    return _count;
  }
  /**
   * Returns true if this value is Null. This method is equivalent to comparing
   * {@link #getType} to {@link BFlatEncoding#Null}.
   * @return true if this value is null, false otherwise.
   */
  public boolean isNull()
  {
    return getType() == BFlatEncoding.Null;
  }

  /**
   * Returns the 8-bit signed integer value of self. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#Int8}.
   * @return the <tt>byte</tt> value of self.
   */
  public byte getInt8()
  {
    assert(getType() == BFlatEncoding.Int8);
    return Fixed.decodeInt8(_data,_dataOffset);
  }
  /**
   * Returns the 8-bit signed integer value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Int8} and <tt>arrayIndex</tt> is less than the
   * return value of {@link #getArrayLength}.
   *
   * @param arrayIndex the 0-based index into this array.
   * @return the <tt>byte</tt> value of this array element.
   */
  public byte getInt8(int arrayIndex)
  {
    assert(getType() == BFlatEncoding.Int8);
    return Fixed.decodeInt8(_data,_dataOffset + arrayIndex);
  }

  /**
   * Returns the 16-bit signed integer value of self. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#Int16}.
   * @return the <tt>short</tt> value of self.
   */
  public short getInt16()
  {
    assert(getType() == BFlatEncoding.Int16);
    return Fixed.decodeInt16(_data,_dataOffset);
  }
  /**
   * Returns the 16-bit signed integer value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Int16} and <tt>arrayIndex</tt> is less than the
   * return value of {@link #getArrayLength}.
   *
   * @param arrayIndex the 0-based index into this array.
   * @return the <tt>short</tt> value of this array element.
   */
  public short getInt16(int arrayIndex)
  {
    assert(getType() == BFlatEncoding.Int16);
    return Fixed.decodeInt16(_data,_dataOffset+(2*arrayIndex));
  }

  /**
   * Returns the 32-bit signed integer value of self. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#Int32}.
   * @return the <tt>int</tt> value of self.
   */
  public int getInt32()
  {
    assert(getType() == BFlatEncoding.Int32);
    return Fixed.decodeInt32(_data,_dataOffset);
  }
  /**
   * Returns the 32-bit signed integer value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Int32} and <tt>arrayIndex</tt> is less than the
   * return value of {@link #getArrayLength}.
   *
   * @param arrayIndex the 0-based index into this array.
   * @return the <tt>int</tt> value of this array element.
   */
  public int getInt32(int arrayIndex)
  {
    assert(getType() == BFlatEncoding.Int32);
    return Fixed.decodeInt32(_data,_dataOffset+(4*arrayIndex));
  }

  /**
   * Returns the 64-bit signed integer value of self. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#Int64}.
   * @return the <tt>long</tt> value of self.
   */
  public long getInt64()
  {
    assert(getType() == BFlatEncoding.Int64);
    return Fixed.decodeInt64(_data,_dataOffset);
  }
  /**
   * Returns the 64-bit signed integer value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Int64} and <tt>arrayIndex</tt> is less than the
   * return value of {@link #getArrayLength}.
   *
   * @param arrayIndex the 0-based index into this array.
   * @return the <tt>long</tt> value of this array element.
   */
  public long getInt64(int arrayIndex)
  {
    assert(getType() == BFlatEncoding.Int64);
    return Fixed.decodeInt64(_data,_dataOffset+(8*arrayIndex));
  }

  /**
   * Returns the 64-bit signed integer value of self. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#Datetime}.
   * @return the <tt>long</tt> value of self.
   */
  public long getDatetime()
  {
    assert(getType() == BFlatEncoding.Datetime);
    return Fixed.decodeInt64(_data,_dataOffset);
  }
  /**
   * Returns the 64-bit signed integer value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Datetime} and <tt>arrayIndex</tt> is less than the
   * return value of {@link #getArrayLength}.
   *
   * @param arrayIndex the 0-based index into this array.
   * @return the <tt>long</tt> value of this array element.
   */
  public long getDatetime(int arrayIndex)
  {
    assert(getType() == BFlatEncoding.Datetime);
    return Fixed.decodeInt64(_data,_dataOffset+(8*arrayIndex));
  }

  /**
   * Returns the double-precision value of self. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#Double}.
   * @return the <tt>double</tt> value of self.
   */
  public double getDouble()
  {
    assert(getType() == BFlatEncoding.Double);
    return Fixed.decodeDouble(_data,_dataOffset);
  }
  /**
   * Returns the double-precision value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Double} and <tt>arrayIndex</tt> is less than the
   * return value of {@link #getArrayLength}.
   *
   * @param arrayIndex the 0-based index into this array.
   * @return the <tt>double</tt> value of this array element.
   */
  public double getDouble(int arrayIndex)
  {
    assert(getType() == BFlatEncoding.Double);
    return Fixed.decodeDouble(_data,_dataOffset+(8*arrayIndex));
  }
  /**
   * Returns the offset into this value's underlying buffer where
   * this value's string or binary data begins. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#String} or
   * {@link BFlatEncoding#Binary}.
   * @return the offset into the underlying buffer where the data for this value begins
   */
  public int getRawValueOffset()
  {
    return (int)(_hint >> 32);
  }
  /**
   * Returns the length of this value's data in bytes.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#String} or {@link BFlatEncoding#Binary}.
   * @return the length, in bytes, of the encoded data for this value
   */
  public int getRawValueLength()
  {
    return (int)(_hint & 0xFFFFFFFF);
  }
  /**
   * Returns this value's string data.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#String} or {@link BFlatEncoding#Binary}.
   * @return the string data for this value
   */
  public String getString()
  {
    return new String(_data,getRawValueOffset(),getRawValueLength(),UTF_8);
  }

  /**
   * Returns the offset into this value's underlying buffer where
   * the specified array element begins. The result of this method
   * is only defined if {@link #getType} is {@link BFlatEncoding#String} or
   * {@link BFlatEncoding#Binary}.
   * @param arrayIndex The 0-based index of the string or binary element
   *                   of self.
   * @return the offset where the specified element begins
   */
  public int getRawValueOffset(int arrayIndex)
  {
    if(arrayIndex==0) return getRawValueOffset();
    return (int)( (_arrayHints[arrayIndex-1]) >> 32 );
  }
  /**
   * Returns the length of the specified array element data in bytes.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#String} or {@link BFlatEncoding#Binary}.
   * @param arrayIndex The 0-based index of the string or binary element
   *                   of self.
   * @return the length of the raw value of the specified element
   */
  public int getRawValueLength(int arrayIndex)
  {
    if(arrayIndex==0) return getRawValueLength();
    return (int)( (_arrayHints[arrayIndex-1]) & 0xFFFFFFFF );
  }
  /**
   * Returns the string value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#String} or {@link BFlatEncoding#Binary}.
   * @param arrayIndex The 0-based index of the string or binary element
   *                   of self.
   * @return the string value of the element at the specified position
   */
  public String getString(int arrayIndex)
  {
    long v = _hint;
    if(arrayIndex>0) v = _arrayHints[arrayIndex-1];
    return new String(_data, (int)(v>>32), (int)(v&0xFFFFFFFF), UTF_8);
  }

  /**
   * Returns the signed integer value of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Leb128}.
   * @return the signed integer equivalent of this value
   */
  public long getLeb128()
  {
    assert(getType() == BFlatEncoding.Leb128);
    return _hint;
  }
  /**
   * Returns the signed integer value of an array element of self.
   * The result of this method is only defined if {@link #getType} is
   * {@link BFlatEncoding#Leb128}.
   * @param arrayIndex The 0-based index of the LEB128-encoded element of self.
   * @return the signed integer value of the specified element
   */
  public long getLeb128(int arrayIndex)
  {
    if(arrayIndex == 0) return _hint;
    return _arrayHints[arrayIndex-1];
  }

  /**
   * Returns the signed integer value of self.
   * This method may be used for any value whose {@link #getType} returns
   * {@link BFlatEncoding#Int8}, {@link BFlatEncoding#Int16},
   * {@link BFlatEncoding#Int32}, {@link BFlatEncoding#Int64},
   * {@link BFlatEncoding#Datetime}, {@link BFlatEncoding#Leb128}.
   * @return The <tt>long</tt> value of self.
   * @throws BFlatException This value is of an unexpected type.
   */
  public long getLong() throws BFlatException
  {
    switch(getType())
    {
      case BFlatEncoding.Int8:
        return getInt8();
      case BFlatEncoding.Int16:
        return getInt16();
      case BFlatEncoding.Int32:
        return getInt32();
      case BFlatEncoding.Int64:
        return getInt64();
      case BFlatEncoding.Datetime:
        return getDatetime();
      case BFlatEncoding.Leb128:
        return getLeb128();
      case BFlatEncoding.Null:
        throw new BFlatException("value is null");
      default:
        throw new BFlatException("cannot convert to integer");
    }
  }
 /**
   * Returns the signed integer value of an array element of self.
   * This method may be used for any value whose {@link #getType} returns
   * {@link BFlatEncoding#Int8}, {@link BFlatEncoding#Int16},
   * {@link BFlatEncoding#Int32}, {@link BFlatEncoding#Int64},
   * {@link BFlatEncoding#Datetime}, {@link BFlatEncoding#Leb128}.
   * @param arrayIndex The 0-based index of the array element of self.
   * @return The <tt>long</tt> value of the specified array element.
   * @throws BFlatException This value is of an unexpected type.
   */
  public long getLong(int arrayIndex) throws BFlatException
  {
    switch(getType())
    {
      case BFlatEncoding.Int8:
        return getInt8(arrayIndex);
      case BFlatEncoding.Int16:
        return getInt16(arrayIndex);
      case BFlatEncoding.Int32:
        return getInt32(arrayIndex);
      case BFlatEncoding.Int64:
        return getInt64(arrayIndex);
      case BFlatEncoding.Datetime:
        return getDatetime(arrayIndex);
      case BFlatEncoding.Leb128:
        return getLeb128(arrayIndex);
      case BFlatEncoding.Null:
        throw new BFlatException("value is null");
      default:
        throw new BFlatException("cannot convert to integer");
    }
  }

  BFlatValue(byte[] data)
  {
    _data = data;
  }

  boolean isReuse()
  {
    return _isReuse;
  }

  BFlatValue reset(byte[] data)
  {
    _data = data;
    _isReuse = false;
    return this;
  }

  void setBuffer(byte[] data)
  {
    _data = data;
  }

  void setTag(int offset, int len)
  {
    _tagOffset = offset;
    _tagLength = len;
  }

  void setData(int position, byte type, int count)
  {
    _dataOffset = position;
    _type = type;
    _count = count;
    if( _count > 1 && (getType() == BFlatEncoding.String || getType() == BFlatEncoding.Binary ||
        getType() == BFlatEncoding.Leb128))
    {
      if(_arrayHints == null || _arrayHints.length < _count-1)
      {
        _arrayHints = new long[_count-1];
      }
    }
  }

  void setStringOffsetAndLen(int pos, int offset, int length)
  {
    long longoffset = offset;
    long val = (longoffset<<32) | length;
    if(pos == 0)
      _hint = val;
    else
      _arrayHints[pos-1] = val;
  }

  void setLeb128Value(int pos, long value)
  {
    if(pos == 0)
      _hint = value;
    else
      _arrayHints[pos-1] = value;
  }

  static Charset UTF_8 = Charset.forName("UTF-8");

  byte[]  _data;
  long[]  _arrayHints;
  long    _hint;
  int     _tagOffset;
  int     _tagLength;
  int     _dataOffset;
  int     _count;
  byte    _type;
  boolean _isReuse;
}
