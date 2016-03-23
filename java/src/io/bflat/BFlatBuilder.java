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
 * BFlatBuilder is a class used to encode values into a byte array of
 * BFlat format. Construct BFlatBuilder with the byte array to encode
 * values into, and then use the <tt>encode</tt> method to encode tag and
 * value pairs into the byte array. To encode arrays, use
 * <tt>encodeTagArray</tt> followed by <tt>encode</tt> to encode the
 * elements of the array.
 * <p>
 * Example of encoding scalar tag/value pairs into BFlat format:
 * <pre>
 * // Allocate an array
 * byte[] outputBuffer = new byte[1024];
 * // Create encoder to encode into the array, starting at index 0
 * BFlatBuilder encoder = new BFlatBuilder(outputBuffer, 0);
 * // Encode { "tag_1": 10, "tag_2": "value"}
 * encoder.encode("tag_1", 10);
 * encoder.encode("tag_2", "value");
 *
 * // The current number of bytes encoded is encoder.position.
 * int finalLength = encoder.position;
 * </pre>
 * <p>
 * Example of encoding an array into BFlat format:
 * <pre>
 * // Allocate an array
 * byte[] outputBuffer = new byte[1024];
 * // Create encoder to encode starting at index 10
 * BFlatBuilder encoder = new BFlatBuilder(outputBuffer, 10);
 * // Encode { "array_1": [0,1,-1,32] }
 * encoder.encodeTagArray(BFlatEncoding.Int32, "array_1", 4);
 * encoder.encode(0).encode(1).encode(-1).encode(32);
 * </pre>
 */
public class BFlatBuilder extends Buffer
{

  /**
   * Construct BFlatBuilder with an existing byte array to write into.
   *
   * @param data     The output byte array for this encoder.
   * @param position The index of <tt>data</tt> to begin writing at.
   */
  public BFlatBuilder(byte[] data, int position)
  {
    this.data = data;
    this.position = position;
    this.start = this.position;
  }

  /**
   * Encode a tag into this byte array. A call to encodeTag must be followed
   * by a call to <tt>encode</tt> unless the <tt>type</tt> is
   * {@link BFlatEncoding#Null}.
   *
   * @param type    One of the type codes from {@link BFlatEncoding}.
   * @param tagName The UTF-8 encoded tag name.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @throws BFlatException          A usage error occurred; typically because
   *                                 the <tt>tagName</tt> was empty.
   * @return This encoder.
   */
  public BFlatBuilder encodeTag(byte type, byte[] tagName)
  {
    if(tagName.length == 0)
    {
      throw new BFlatException("Zero length tags are not allowed.");
    }
    int tagLen = tagName.length;
    if(tagLen < 8)
    {
      // fit the tag len into the byte0
      data[position++] = (byte)(type | tagLen);
      System.arraycopy(tagName,0,data,position,tagLen);
      position += tagLen;
    }
    else
    {
      data[position++] = type;
      Leb128.encodeUnsigned(this, tagLen);
      System.arraycopy(tagName,0,data,position,tagLen);
      position += tagLen;
    }
    return this;
  }

  /**
   * Encode a tag into this byte array. A call to encodeTag must be followed
   * by a call to <tt>encode</tt> unless the <tt>type</tt> is
   * {@link BFlatEncoding#Null}.
   *
   * @param type    One of the type codes from {@link BFlatEncoding}.
   * @param tagName The tag name.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @throws BFlatException          A usage error occurred; typically because
   *                                 the <tt>tagName</tt> was empty.
   * @return This encoder.
   */
  public BFlatBuilder encodeTag(byte type, String tagName)
  {
    if(tagName.length() == 0)
    {
      throw new BFlatException("Zero length tags are not allowed.");
    }
    int tagLen = tagName.length();
    if(tagLen < 8)
    {
      // see if fit the tag len into the byte0
      int byte0 = position++;
      int actual = encodeString(tagName);
      if(actual<8)
      {
        data[byte0] = (byte)(type | actual);
      }
      else
      {
        // since the tag <8 characters we know that the # of bytes
        // it will fit in is representable with a one byte leb128
        data[byte0] = (byte)type;
        System.arraycopy(data,byte0+1,data,byte0+2,actual);
        data[byte0+1] = (byte)actual;
        ++position;
      }
    }
    else
    {
      data[position++] = (byte)type;
      int start = position;
      int leblength = Leb128.encodeUnsigned(this, tagLen);
      int actual = encodeString(tagName);
      if(actual != tagLen)
      {
        int end = position;
        position = start;
        int actual_leblength = Leb128.encodeUnsigned(this,actual);
        // do we need to reserialize the data?
        if(actual_leblength != leblength)
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

  /**
   * Encode an array tag into this byte array. A call to this method must
   * be followed by <tt>count</tt> calls to <tt>encode</tt>.
   *
   * @param type    One of the type codes from {@link BFlatEncoding}.
   * @param tagName The UTF-8 encoded tag name.
   * @param count   The count of elements in this array. The number of calls
   *                to <tt>encode</tt> must match this value, or a malformed
   *                BFlat document will be produced.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @throws BFlatException          A usage error occurred; typically because
   *                                 the <tt>tagName</tt> was empty.
   * @return This encoder.
   */
  public BFlatBuilder
  encodeTagArray(byte type, byte[] tagName, int count)
  {
    encodeTag((byte)(type|BFlatEncoding.ArrayMask),tagName);
    Leb128.encodeUnsigned(this,count);
    return this;
  }

  /**
   * Encode an array tag into this byte array. A call to this method must
   * be followed by <tt>count</tt> calls to <tt>encode</tt>.
   *
   * @param type    One of the type codes from {@link BFlatEncoding}.
   * @param tagName The tag name.
   * @param count   The count of elements in this array. The number of calls
   *                to <tt>encode</tt> must match this value, or a malformed
   *                BFlat document will be produced.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @throws BFlatException          A usage error occurred; typically because
   *                                 the <tt>tagName</tt> was empty.
   * @return This encoder.
   */
  public BFlatBuilder
  encodeTagArray(byte type, String tagName, int count)
  {
    encodeTag((byte)(type|BFlatEncoding.ArrayMask), tagName);
    Leb128.encodeUnsigned(this,count);
    return this;
  }

  /**
   * Copy a previously-encoded UTF-8 string value into this byte array.
   * A call to this method must be preceded by a call to <tt>encodeTag</tt> or
   * <tt>encodeTagArray</tt>. The caller must ensure the byte array is a valid
   * UTF-8 encoded string, no checking is performed.
   *
   * @param value  The byte array containing a UTF-8 string to be included
   *               in the output message.
   * @param offset The offset in <tt>value</tt> where the encoded UTF-8
   *               string begins.
   * @param length The length of the UTF-8 encoded string, in bytes, to
   *               be written into the output message.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder
  encode(byte[] value, int offset, int length)
  {
    Leb128.encodeUnsigned(this,length);
    System.arraycopy(value,offset,data,position,length);
    position += length;
    return this;
  }
  /**
   * Encode a string value into this byte array. A call to this method
   * must be preceded by a call to <tt>encodeTag</tt> or
   * <tt>encodeTagArray</tt>.
   *
   * @param value The {@link String} value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder
  encode(String value)
  {
    int charcount = value.length();
    int start = position;
    int leblength = Leb128.encodeUnsigned(this,charcount);
    int bytecount = encodeString(value);
    int end = position;
    if(bytecount != charcount) // non ascii
    {
      position = start;
      int actual_leblength = Leb128.encodeUnsigned(this,bytecount);
      if(actual_leblength != leblength)
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
  /**
   * Encode a byte value into this byte array. A call to this method
   * must be preceded by a call to <tt>encodeTag</tt> or
   * <tt>encodeTagArray</tt>.
   *
   * @param value The byte value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder
  encode(byte value)
  {
    position += Fixed.encodeInt8(value,data,position);
    return this;
  }
  /**
   * Encode a short value into this byte array. A call to this method
   * must be preceded by a call to <tt>encodeTag</tt> or
   * <tt>encodeTagArray</tt>.
   *
   * @param value The <tt>short</tt> value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder
  encode(short value)
  {
    position += Fixed.encodeInt16(value,data,position);
    return this;
  }
  /**
   * Encode an integer value into this byte array. A call to this method
   * must be preceded by a call to <tt>encodeTag</tt> or
   * <tt>encodeTagArray</tt>.
   *
   * @param value The <tt>int</tt> value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder
  encode(int value)
  {
    position += Fixed.encodeInt32(value,data,position);
    return this;
  }
  /**
   * Encode a long integer value into this byte array. A call to this method
   * must be preceded by a call to <tt>encodeTag</tt> or
   * <tt>encodeTagArray</tt>.
   *
   * @param value The <tt>long</tt> value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder
  encode(long value)
  {
    position += Fixed.encodeInt64(value,data,position);
    return this;
  }
  /**
   * Encode a double-precision floating point value into this byte array.
   * A call to this method must be preceded by a call to <tt>encodeTag</tt> or
   * <tt>encodeTagArray</tt>.
   *
   * @param value The <tt>double</tt> value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder
  encode(double value)
  {
    position += Fixed.encodeDouble(value,data,position);
    return this;
  }

  /**
   * Encode a tag and string value into this byte array.
   *
   * @param tagName The tag name.
   * @param value The {@link String} value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder encode(String tagName, String value)
  {
    encodeTag(BFlatEncoding.String, tagName);
    encode(value);
    return this;
  }

  /**
   * Encode a tag and byte value into this byte array.
   * @param tagName The tag name.
   * @param value The byte value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder encode(String tagName, byte value)
  {
    encodeTag(BFlatEncoding.Int8, tagName);
    encode(value);
    return this;
  }

  /**
   * Encode a tag and short value into this byte array.
   * @param tagName The tag name.
   * @param value The short value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder encode(String tagName, short value)
  {
    encodeTag(BFlatEncoding.Int16, tagName);
    encode(value);
    return this;
  }

  /**
   * Encode a tag and int value into this byte array.
   * @param tagName The tag name.
   * @param value The int value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder encode(String tagName, int value)
  {
    encodeTag(BFlatEncoding.Int32, tagName);
    encode(value);
    return this;
  }

  /**
   * Encode a tag and long value into this byte array.
   * @param tagName The tag name.
   * @param value The long value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder encode(String tagName, long value)
  {
    encodeTag(BFlatEncoding.Int64, tagName);
    encode(value);
    return this;
  }

  /**
   * Encode a tag and double value into this byte array.
   * @param tagName The tag name.
   * @param value The double value to encode into this array.
   *
   * @throws BufferTooSmallException The provided buffer is not large enough
   *                                 to encode this tag. Create a new
   *                                 BFlatBuilder with a larger output buffer.
   * @return This encoder.
   */
  public BFlatBuilder encode(String tagName, double value)
  {
    encodeTag(BFlatEncoding.Double, tagName);
    encode(value);
    return this;
  }

  //
  // This utility method performs a fast UTF-16 to UTF-8 conversion manually
  // but falls back to java when we have surrogate pairs to deal with.
  //
  private int
  encodeString(String string)
  {
    // at least a couple of approaches we could take, but with
    // hotspot this ends up being the fastest over time with 0 garbage
    int length = string.length();
    int i = 0;
    int start = position;
    for(;i < length; ++i)
    {
      char c = string.charAt(i);
      if(c<128) data[position++] = (byte)c;
      else break;
    }
    while(i<length)
    {
      char c = string.charAt(i++);
      if(c<=127) data[position++] = (byte)c;
      else if(c <= 0x7ff)
      {
        data[position++] = (byte)(0xc0 | (c>>6));
        data[position++] = (byte)(0x80 | (c&0x3f));
      }
      else if (Character.isSurrogate(c))
      {
        // fall back to java version of this
        position = start;
        byte[] bytes = string.getBytes(UTF_8);
        System.arraycopy(bytes,0,data,position,bytes.length);
        position += length;
        return length;
      }
      else
      {
        data[position++] = (byte)(0xe0 | (c>>12));
        data[position++] = (byte)(0x80 | ((c>>6)&0x3f));
        data[position++] = (byte)(0x80 | (c&0x3f));
      }
    }
    return position - start;
  }

  private static Charset UTF_8 = Charset.forName("UTF-8");
}
