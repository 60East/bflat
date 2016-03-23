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
 * A set of utility functions for encoding and decoding fixed-width integers.
 */
public class Fixed
{
  /**
   * Encodes an 8-bit integer.
   * @param value    The value to encode
   * @param output   The destination array
   * @param position The position in <tt>output</tt> to write to.
   * @return The number of bytes written to <tt>output</tt>.
   *
   * @throws BufferTooSmallException Not enough space remaining
   *                                 in <tt>output</tt>.
   */
  public static int encodeInt8(byte value, byte[] output, int position)
    throws BufferTooSmallException
  {
    try
    {
      output[position] = value;
      return 1;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BufferTooSmallException("while encoding int8");
    }
  }

  /**
   * Decodes an 8-bit integer.
   * @param source   The source array for this data.
   * @param position The position in <tt>source</tt> to decode from.
   * @return The decoded value.
   *
   * @throws BFlatException Not enough bytes in <tt>source</tt> to represent
   *                        this value.
   */
  public static byte decodeInt8(byte[] source, int position)
    throws BFlatException
  {
    try
    {
      return source[position];
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BFlatException("invalid int8 value", position);
    }
  }

  /**
   * Encodes a 16-bit integer.
   * @param value    The value to encode
   * @param output   The destination array
   * @param position The position in <tt>output</tt> to write to.
   * @return The number of bytes written to <tt>output</tt>.
   *
   * @throws BufferTooSmallException Not enough space remaining
   *                                 in <tt>output</tt>.
   */
  public static int encodeInt16(short value, byte[] output, int position)
    throws BufferTooSmallException
  {
    try
    {
      output[position]   = (byte) ( value & 0xff );
      output[position+1] = (byte) ( (value & 0xff00) >> 8 );
      return 2;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BufferTooSmallException("while encoding int16");
    }
  }

  /**
   * Decodes a 16-bit integer.
   * @param source   The source array for this data.
   * @param position The position in <tt>source</tt> to decode from.
   * @return The decoded value.
   *
   * @throws BFlatException Not enough bytes in <tt>source</tt> to represent
   *                        this value.
   */
  public static short decodeInt16(byte[] source, int position)
    throws BFlatException
  {
    try
    {
      short value = 0;
      int shift = 0;
      for(int i = 0; i < 2; ++i)
      {
        short mask = 0xff;
        short val = (short)(source[position+i]&mask);
        value |= val<<(i*8);
      }
      return value;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BFlatException("invalid int16 value", position);
    } 
  }

  /**
   * Encodes a 32-bit integer.
   * @param value    The value to encode
   * @param output   The destination array
   * @param position The position in <tt>output</tt> to write to.
   * @return The number of bytes written to <tt>output</tt>.
   *
   * @throws BufferTooSmallException Not enough space remaining
   *                                 in <tt>output</tt>.
   */
  public static int encodeInt32(int value, byte[] output, int position)
    throws BufferTooSmallException
  {
    try
    {
      output[position]   = (byte) (  value & 0xff );
      output[position+1] = (byte) ( (value & 0xff00) >> 8 );
      output[position+2] = (byte) ( (value & 0xff0000) >> 16 );
      output[position+3] = (byte) ( (value & 0xff000000) >> 24 );
      return 4;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BufferTooSmallException("while encoding int32");
    }
  }

  /**
   * Decodes a 32-bit integer.
   * @param source   The source array for this data.
   * @param position The position in <tt>source</tt> to decode from.
   * @return The decoded value.
   *
   * @throws BFlatException Not enough bytes in <tt>source</tt> to represent
   *                        this value.
   */
  public static int decodeInt32(byte[] source, int position)
    throws BFlatException
  {
    try
    {
      int value = 0;
      int shift = 0;
      for(int i = 0; i < 4; ++i)
      {
        int val = source[position+i]&0xff;
        value |= val<<(i*8);
      }
      return value;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BFlatException("invalid int32 value", position);
    } 
  }
  /**
   * Encodes a 64-bit integer.
   * @param value    The value to encode
   * @param output   The destination array
   * @param position The position in <tt>output</tt> to write to.
   * @return The number of bytes written to <tt>output</tt>.
   *
   * @throws BufferTooSmallException Not enough space remaining
   *                                 in <tt>output</tt>.
   */
  public static int encodeInt64(long value, byte[] output, int position)
    throws BufferTooSmallException
  {
    try
    {
      output[position]   = (byte) (  value & 0xff );
      output[position+1] = (byte) ( (value & 0xff00)             >> 8 );
      output[position+2] = (byte) ( (value & 0xff0000)           >> 16 );
      output[position+3] = (byte) ( (value & 0xff000000)         >> 24 );
      output[position+4] = (byte) ( (value & 0xff00000000L)      >> 32 );
      output[position+5] = (byte) ( (value & 0xff0000000000L)    >> 40 );
      output[position+6] = (byte) ( (value & 0xff000000000000L)  >> 48 );
      output[position+7] = (byte) ( (value & 0xff00000000000000L)>> 56 );
      return 8;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BufferTooSmallException("while encoding int64");
    }
  }

  /**
   * Decodes a 64-bit integer.
   * @param source   The source array for this data.
   * @param position The position in <tt>source</tt> to decode from.
   * @return The decoded value.
   *
   * @throws BFlatException Not enough bytes in <tt>source</tt> to represent
   *                        this value.
   */
  public static long decodeInt64(byte[] source, int position)
    throws BFlatException
  {
    try
    {
      long value = 0;
      int shift = 0;
      for(int i = 0; i < 8; ++i)
      {
        long val = source[position+i]&0xff;
        value |= val<<(i*8);
      }
      return value;
    }
    catch(IndexOutOfBoundsException aex)
    {
      throw new BFlatException("invalid int64 value", position);
    } 
  }

  /**
   * Encodes a double-precision floating point value.
   * @param value    The value to encode
   * @param output   The destination array
   * @param position The position in <tt>output</tt> to write to.
   * @return The number of bytes written to <tt>output</tt>.
   *
   * @throws BufferTooSmallException Not enough space remaining
   *                                 in <tt>output</tt>.
   */
  public static int encodeDouble(double value, byte[] output, int position)
    throws BufferTooSmallException
  {
    long dval = Double.doubleToLongBits(value);
    return encodeInt64(dval,output,position);
  }

  /**
   * Decodes a double-precision floating point value.
   * @param source   The source array for this data.
   * @param position The position in <tt>source</tt> to decode from.
   * @return The decoded value.
   *
   * @throws BFlatException Not enough bytes in <tt>source</tt> to represent
   *                        this value.
   */
  public static double decodeDouble(byte[] source, int position)
    throws BFlatException
  {
    long dval = decodeInt64(source,position);
    return Double.longBitsToDouble(dval);
  }

  private Fixed()
  {
  }
}
