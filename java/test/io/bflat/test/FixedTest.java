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
package io.bflat.test;
import static org.junit.Assert.*;
import org.junit.Test;
import io.bflat.Fixed;

public class FixedTest
{
  @Test
  public void int8() throws Exception
  {
    byte[] buf = new byte[1];
    byte data = 0;
    assertEquals(1, Fixed.encodeInt8(data,buf,0));
    assertEquals(data, Fixed.decodeInt8(buf,0));

    data = 1;
    Fixed.encodeInt8(data,buf,0);
    assertEquals(data, Fixed.decodeInt8(buf,0));

    data = (byte)0xff;
    Fixed.encodeInt8(data,buf,0);
    assertEquals(data, Fixed.decodeInt8(buf,0));
  }

  @Test
  public void int16() throws Exception
  {
    byte[] buf = new byte[2];
    short data = 0;

    assertEquals(2, Fixed.encodeInt16(data,buf,0));
    assertEquals(data, Fixed.decodeInt16(buf,0));

    data = 2;
    assertEquals(2, Fixed.encodeInt16(data,buf,0));
    assertEquals(2, buf[0]);
    assertEquals(0, buf[1]);
    assertEquals(data, Fixed.decodeInt16(buf,0));

    data = -2;
    assertEquals(2, Fixed.encodeInt16(data,buf,0));
    assertEquals((byte)0xfe, buf[0]);
    assertEquals((byte)0xff, buf[1]);
    assertEquals(data, Fixed.decodeInt16(buf, 0));
  }

  @Test
  public void int32() throws Exception
  {
    byte[] buf = new byte[4];
    int data =0;
    assertEquals(4, Fixed.encodeInt32(data,buf,0));
    assertEquals(data,Fixed.decodeInt32(buf,0));

    data = -1;
    assertEquals(4, Fixed.encodeInt32(data,buf,0));
    assertEquals(data,Fixed.decodeInt32(buf,0));
    data = Integer.MIN_VALUE;
    assertEquals(4, Fixed.encodeInt32(data,buf,0));
    assertEquals(data,Fixed.decodeInt32(buf,0));
    data = Integer.MAX_VALUE;
    assertEquals(4, Fixed.encodeInt32(data,buf,0));
    assertEquals(data,Fixed.decodeInt32(buf,0));

  }
  @Test
  public void int64() throws Exception
  {
    byte[] buf = new byte[8];
    long data = 0xfffefdfcfbfaf9L;

    assertEquals(8, Fixed.encodeInt64(data,buf,0));
    assertEquals((byte)0xf9,buf[0]); 
    assertEquals((byte)0xfa,buf[1]); 
    assertEquals((byte)0xfb,buf[2]); 
    assertEquals((byte)0xfc,buf[3]); 
    assertEquals((byte)0xfd,buf[4]); 
    assertEquals((byte)0xfe,buf[5]); 
    assertEquals((byte)0xff,buf[6]); 
    assertEquals((byte)0x00,buf[7]); 
    assertEquals(data, Fixed.decodeInt64(buf,0));

    data *= -1;
    assertEquals(8, Fixed.encodeInt64(data,buf,0));
    assertEquals((byte)0x07,buf[0]); 
    assertEquals((byte)0x05,buf[1]); 
    assertEquals((byte)0x04,buf[2]); 
    assertEquals((byte)0x03,buf[3]); 
    assertEquals((byte)0x02,buf[4]); 
    assertEquals((byte)0x01,buf[5]); 
    assertEquals((byte)0x00,buf[6]); 
    assertEquals((byte)0xff,buf[7]); 
    assertEquals(data, Fixed.decodeInt64(buf,0));
  }

  @Test
  public void dbl() throws Exception
  {
    byte[] buf = new byte[8];
    double data = -1.23;

    assertEquals(8, Fixed.encodeDouble(data,buf,0));
    assertEquals((byte)0xae,buf[0]);
    assertEquals((byte)'G', buf[1]);
    assertEquals((byte)0xe1,buf[2]);
    assertEquals((byte)'z', buf[3]);
    assertEquals((byte)0x14,buf[4]);
    assertEquals((byte)0xae,buf[5]);
    assertEquals((byte)0xf3,buf[6]);
    assertEquals((byte)0xbf,buf[7]);
    assertEquals(data,Fixed.decodeDouble(buf,0),0.01);

    data *= -20;
    assertEquals(8, Fixed.encodeDouble(data,buf,0));
    assertEquals(data,Fixed.decodeDouble(buf,0),0.01);
  }
}

