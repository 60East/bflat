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
import io.bflat.Leb128;
import io.bflat.Buffer;


public class Leb128Test
{
  @Test
  public void encodeSignedTest() throws Exception
  {
    byte[] buf = new byte[2];
    Buffer output = new Buffer(buf,0);

    assertEquals(1,Leb128.encodeSigned(output,1));
    assertEquals(0x1, buf[0]);
    assertEquals(1,Leb128.encodeSigned(output.rewind(),0));
    assertEquals(0x0, buf[0]);
    assertEquals(1,Leb128.encodeSigned(output.rewind(),-1));
    assertEquals(0x7f,buf[0]);
    assertEquals(2,Leb128.encodeSigned(output.rewind(),127));
    assertEquals((byte)0xff,buf[0]);
    assertEquals(0x00,buf[1]);
    assertEquals(2,Leb128.encodeSigned(output.rewind(),-128));
    assertEquals((byte)0x80,buf[0]);
    assertEquals(0x7f,buf[1]);

    buf = new byte[3];
    output.data = buf;

    assertEquals(3,Leb128.encodeSigned(output.rewind(),-32767));
    assertEquals((byte)0x81,buf[0]);
    assertEquals((byte)0x80,buf[1]);
    assertEquals((byte)0x7e,buf[2]);

    buf = new byte[4];
    output.data = buf;
    output.position = 1;

    assertEquals(3,Leb128.encodeSigned(output,32768));
    assertEquals((byte)0x80,buf[1]);
    assertEquals((byte)0x80,buf[2]);
    assertEquals((byte)0x02,buf[3]);
  }

  @Test
  public void decodeSignedTest() throws Exception
  {
    Buffer buffer = new Buffer(new byte[10],0);

    long data = 0;
    Leb128.encodeSigned(buffer,data);
    assertEquals(1,buffer.position);
    assertEquals(data, Leb128.decodeSigned(buffer));
    data = 1;
    Leb128.encodeSigned(buffer.rewind(),data);
    assertEquals(1,buffer.position);
    assertEquals(data, Leb128.decodeSigned(buffer.rewind()));
    data = -1;
    Leb128.encodeSigned(buffer.rewind(),data);
    assertEquals(1,buffer.position);
    assertEquals(data, Leb128.decodeSigned(buffer.rewind()));
    data = 127;
    Leb128.encodeSigned(buffer.rewind(),data);
    assertEquals(2,buffer.position);
    assertEquals(data, Leb128.decodeSigned(buffer.rewind()));
    data = -128;
    Leb128.encodeSigned(buffer.rewind(),data);
    assertEquals(2,buffer.position);
    assertEquals(data, Leb128.decodeSigned(buffer.rewind()));
    data = 32767;
    Leb128.encodeSigned(buffer.rewind(),data);
    assertEquals(3,buffer.position);
    assertEquals(data, Leb128.decodeSigned(buffer.rewind()));
    data = -32768;
    Leb128.encodeSigned(buffer.rewind(),data);
    assertEquals(3,buffer.position);
    assertEquals(data, Leb128.decodeSigned(buffer.rewind()));
  }

  @Test
  public void encodeUnsignedTest() throws Exception
  {
    Buffer buf = new Buffer(new byte[10],0);
    assertEquals(1,Leb128.encodeUnsigned(buf,0));
    assertEquals(0,buf.data[0]);

    assertEquals(1,Leb128.encodeUnsigned(buf,1));
    assertEquals(1,buf.data[1]);

    assertEquals(1,Leb128.encodeUnsigned(buf.rewind(),127));
    assertEquals((byte)0x7f,buf.data[0]);

    assertEquals(2,Leb128.encodeUnsigned(buf.rewind(),128));
    assertEquals((byte)0x80,buf.data[0]);
    assertEquals((byte)0x01,buf.data[1]);

  }


}

