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
import java.util.*;

/**
 * Parser for BFlat messages that provides an {@link Iterator} interface
 * to the values in a message. BFlatParser parses BFlat-encoded data from
 * a byte array acting as an iterator over the values contained in the
 * message.
 * <p>
 * Example of parsing a BFlat message using Java range-for syntax:
 * <pre>
 * // Assume rawMessage contains only a BFlat-encoded message with scalar
 * //  values -- see {@link BFlatValue} for more information on using arrays.
 * byte[] rawMessage = ...;
 * BFlatParser parser = new BFlatParser();
 *
 * for(BFlatValue value : parser.parse(rawMessage))
 * {
 *   // Note, if you just want to convert the value to a string,
 *   //  you could simply say 'value.toString()'.
 *   //
 *   // print the "tag" string from this value.
 *   System.out.printf("%s: ", value.getTag());
 *   // Use the type to invoke the right getter and format correctly.
 *   switch(value.getType())
 *   {
 *       case BFlatEncoding.String:
 *       case BFlatEncoding.Binary:
 *         System.out.printf("%s%n", value.getString());
 *         break;
 *       case BFlatEncoding.Int8:
 *       case BFlatEncoding.Int16:
 *       case BFlatEncoding.Int32:
 *       case BFlatEncoding.Int64:
 *       case BFlatEncoding.Leb128:
 *       case BFlatEncoding.Datetime:
 *         System.out.printf("%l%n", value.getLong());
 *         break;
 *       case BFlatEncoding.Null:
 *         System.out.println("null");
 *         break;
 *       case BFlatEncoding.Double:
 *         System.out.printf("%f%n", value.getDouble());
 *         break;
 *   }
 *   // Optional -- since we are finished with <tt>value</tt>, calling
 *   //  {@link BFlatValue#reuse} allows it to be reused by the parser
 *   //  for better performance/less garbage creation.
 *   value.reuse();
 * }
 * </pre>
 */
public class BFlatParser extends Buffer implements Iterable<BFlatValue>,
                                                   Iterator<BFlatValue>
{
  /**
   * Construct a default <tt>BFlatParser</tt>. The {@link #parse(byte[])}
   * or {@link #parse(byte[],int,int)} method must be called before iteration
   * can begin.
   */
  public BFlatParser()
  {
  }

  /**
   * Begin parsing a BFlat message contained in a byte array.
   * Upon successful return, this parser object may be used to iterate over
   * the values in the message.
   *
   * @param data The byte array containing a BFlat-encoded message to parse.
   *
   * @return This parser object, which may be used as an iterator over the
   *         values in this message.
   * @throws BFlatException An error occurred parsing this message.
   */
  public BFlatParser parse(byte[] data)
  {
    return parse(data,0,data.length);
  }

  /**
   * Begin parsing a BFlat message contained in a byte array.
   * Upon successful return, this parser object may be used to iterate over
   * the values in the message.
   *
   * @param data The byte array containing a BFlat-encoded message to parse.
   * @param position The position in <tt>data</tt> where BFlat data begins.
   * @param length The length of BFlat data contained in <tt>data</tt>.
   *
   * @return This parser object, which may be used as an iterator over the
   *         values in this message.
   * @throws BFlatException An error occurred parsing this message.
   */
  public BFlatParser parse(byte[] data, int position, int length)
  {

    this.data = data;
    this.position = position;
    this.start = this.position;

    _end = position + length;
    return this;
  }

  /**
   * Returns an iterator over the parsed values in this message.
   */
  public Iterator<BFlatValue> iterator()
  {
    return this;
  }

  /**
   * Returns true if another value is present in this BFlat message.
   *
   * @return true if a value is remaining, false otherwise.
   */
  public boolean hasNext()
  {
    return position < _end;
  }


  public void remove()
  {
    throw new UnsupportedOperationException();
  }

  /**
   * Returns the next value from this BFlat message.
   *
   * @return The next value in this message.
   * @throws BFlatException An error occurred parsing this message.
   */
  public BFlatValue next()
  {
    _prev = parseNext();
    return _prev;
  }

  BFlatValue parseNext()
    throws BFlatException
  {
    if(position >= _end)
    {
      return null;
    }
    BFlatValue value = (_prev!=null && _prev.isReuse()) 
      ? _prev.reset(data) : new BFlatValue(data);

    byte byte0 = data[position++];
    byte type = (byte)(byte0 & BFlatEncoding.TypeMask);
    boolean isArray = (byte0 & BFlatEncoding.ArrayMask) != 0;
    int tagLength = byte0 & BFlatEncoding.LengthMask;
    int tagStart = position;

    if(tagLength == 0)
    {
      tagLength = (int)(Leb128.decodeUnsigned(this));
      if(tagLength == 0)
      {
        throw new BFlatException("zero-length tag",position);
      }
      tagStart = position;
    }
    value.setTag(tagStart,tagLength);
    position += tagLength;
    int dataStart = position;

    int elementCount = 1;
    if(isArray)
    {
      elementCount = (int)(Leb128.decodeUnsigned(this));
      dataStart = position;
    }

    value.setData(position,byte0,elementCount);
    // for variable length types we have to parse the array contents
    switch(type)
    {
      case BFlatEncoding.String:
      case BFlatEncoding.Binary:
        for(int i = 0; i < elementCount; ++i)
        {
          int length = (int)(Leb128.decodeUnsigned(this));
          value.setStringOffsetAndLen(i,position,length);
          position += length;
        }
        break;
      case BFlatEncoding.Leb128:
        for(int i =0; i < elementCount; ++i)
        {
          long leb128 = Leb128.decodeSigned(this);
          value.setLeb128Value(i,leb128);
        }
        break;
      case BFlatEncoding.Int8:
        position += elementCount;
        break;
      case BFlatEncoding.Int16:
        position += elementCount*2;
        break;
      case BFlatEncoding.Int32:
        position += elementCount*4;
        break;
      case BFlatEncoding.Int64:
      case BFlatEncoding.Double:
      case BFlatEncoding.Datetime:
        position += elementCount*8;
      case BFlatEncoding.Null:
        break;
      default:
        throw new BFlatException("unknown value type", position);
    }
    return value;
  }

  BFlatValue _prev;
  int        _end;
}
