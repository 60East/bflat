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

/**
 * Implementation of the LEB128 integer format which is
 * used by BFlat to represent lengths and integers.
 */
public class Leb128
{
  private static final int bits_per_digit = 7;

  /**
   * Decode a signed LEB128 value.
   * @param source The source data of this value.
   * @return The signed LEB128 value found in the source.
   * @throws BFlatException Not enough bytes in the source for a LEB128 value.
   */
  public static long decodeSigned(Buffer source)
    throws BFlatException
  {
    try
    {
      long result = 0;
      int shift = 0;
      while((source.data[source.position] & 0x80) != 0)
      {
        long data = (source.data[source.position++] & 0x7f) << shift;
        shift += bits_per_digit;
        result |= data;
      }
      result |= source.data[source.position] << shift;
      if((source.data[source.position++] & 0x40) != 0)
        result -= (1<<(shift+bits_per_digit));

      return result;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BFlatException("invalid signed leb128 value",source.position);
    }
  }

  /**
   * Encode a signed LEB128 value.
   * @param output The destination to write the encoded value to.
   * @param value The signed value to write to <tt>output</tt>.
   * @return The number of bytes written to <tt>output</tt>.
   * @throws BufferTooSmallException Not enough space remaining
   *                                 in <tt>output</tt>.
   */
  public static int encodeSigned(Buffer output, long value)
    throws BufferTooSmallException
  {
    try
    {
      boolean isMore = true;
      int start = output.position;
      while (isMore)
      {
        byte thisByte = (byte)(value & 0x7f);
        value >>= 7; 
        isMore = !((((value == 0 ) && ((thisByte & 0x40) == 0)) ||
                    ((value == -1) && ((thisByte & 0x40) != 0))));
        if (isMore) thisByte |= 0x80;
        output.data[output.position++] = thisByte;
      }
      return output.position-start;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BufferTooSmallException("out of buffer while encoding leb128");
    }
  }

  /**
   * Decode an unsigned LEB128 value.
   * @param source The source data of this value.
   * @return The unsigned LEB128 value found in the source.
   * @throws BFlatException Not enough bytes in the source for a LEB128 value.
   */
  public static long decodeUnsigned(Buffer source)
    throws BFlatException
  {
    try
    {
      long result = 0;
      int shift = 0;
      while((source.data[source.position] & 0x80) != 0)
      {
        long data = (source.data[source.position++]&0x7f) << shift;
        shift += bits_per_digit;
        result |= data;
      }
      result |= source.data[source.position++]<<shift;
      return result;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BFlatException("invalid unsigned leb128 value",source.position);
    }
  }

  /**
   * Encode an unsigned LEB128 value.
   * @param output The destination to write the encoded value to.
   * @param value The signed value to write to <tt>output</tt>.
   * @return The number of bytes written to <tt>output</tt>.
   * @throws BufferTooSmallException Not enough space remaining
   *                                 in <tt>output</tt>.
   */
  public static int encodeUnsigned(Buffer output, long value)
    throws BufferTooSmallException
  {
    try
    {
      boolean isMore = true;
      int start = output.position;
      while(isMore)
      {
        byte thisByte = (byte)(value&0x7f);
        value>>=7;
        isMore = value!=0;
        if(isMore) thisByte |= 0x80;
        output.data[output.position++] = thisByte;
      }
      return output.position-start;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BufferTooSmallException("out of buffer while encoding leb128");
    }
  }

  private Leb128()
  {
  }
}
