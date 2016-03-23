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
 * BFlatEncoding contains constant values specific to the BFlat encoding type.
 * Values from this class are used to specify data types when using
 * {@link BFlatValue} and {@link BFlatBuilder}.
 */
public class BFlatEncoding
{
  static final byte TypeMask   = 0b01111000;
  static final byte LengthMask = 0b00000111;
  static final byte ArrayMask  = (byte)0b10000000;

  /**
   * Represents a NULL value in BFlat.
   */
  public static final byte Null       = 0b00000000;

  /**
   * Represents a String value in BFlat.
   */
  public static final byte String     = 0b00001000;

  /**
   * Represents a binary value in BFlat.
   */
  public static final byte Binary     = 0b00010000;

  /**
   * Represents an 8-bit signed integer in BFlat.
   */
  public static final byte Int8       = 0b00011000;

  /**
   * Represents a 16-bit signed integer in BFlat.
   */
  public static final byte Int16      = 0b00100000;

  /**
   * Represents a 32-bit signed integer in BFlat.
   */
  public static final byte Int32      = 0b00101000;

  /**
   * Represents a 64-bit signed integer in BFlat.
   */
  public static final byte Int64      = 0b00110000;

  /**
   * Represents a IEEE-754 double-precision value in BFlat.
   */
  public static final byte Double     = 0b00111000;

  /**
   * Represents a 64-bit integer datetime value in BFlat.
   */
  public static final byte Datetime   = 0b01000000;

  /**
   * Represents a variable length signed integer value up to 128-bits in
   * precision, in BFlat.
   */
  public static final byte Leb128     = 0b01001000;


  private BFlatEncoding()
  {
  }
}


