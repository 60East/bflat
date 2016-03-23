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
 * Simple wrapper around a byte array and a current position in the array.
 */
public class Buffer
{
  /**
   * Constuct a null Buffer. Self's <tt>data</tt> field must be set before
   * it can be used.
   */
  public Buffer()
  {
  }

  /**
   * Constuct a Buffer on an existing array.
   * @param data The existing byte array to wrap in this Buffer
   * @param start The 0-based index to begin this buffer at.
   */
  public Buffer(byte[] data, int start)
  {
    this.data = data;
    this.position = start;
    this.start = start;
  }

  /**
   * Returns the number of bytes remaining in this buffer. This value
   * is calculated as the distance between the underlying array's length
   * and the current position.
   * @return the number of bytes remaining in this buffer
   */
  public int remaining()
  {
    return data.length - position;
  }

  /**
   * Rewind this buffer to the original start. Frequently used to overwrite
   * a buffer after an encode operation.
   * @return This Buffer.
   */
  public Buffer rewind()
  {
    this.position = this.start;
    return this;
  }

  /**
   * The underlying byte array of self.
   */
  public byte[] data;
  /**
   * The current position in <tt>data</tt> of self.
   */
  public int position;
  /**
   * The user-specified starting position for data in this Buffer.
   */
  public int start;
}
