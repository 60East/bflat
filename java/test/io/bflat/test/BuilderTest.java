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
import io.bflat.*;
import java.util.*;

public class BuilderTest
{
  @Test
  public void encodeInt8() throws Exception
  {
    final byte testData[] = {0, -1, 1, 127, -128};
    BFlatBuilder encoder = new BFlatBuilder(new byte[1024],0);
    for(byte value : testData)
    {
      encoder.encodeTag(BFlatEncoding.Int8, "value");
      encoder.encode(value);
    }

    BFlatParser parser = new BFlatParser();

    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getInt8(), testData[index++]);
      assertEquals(value.getTag(), "value");
    }
  }
  @Test
  public void encodeInt16() throws Exception
  {
    final short testData[] = {0, -1, 1, 127, -128, 128, -129, -32768,32767};
    BFlatBuilder encoder = new BFlatBuilder(new byte[1024],0);
    for(short value : testData)
    {
      encoder.encodeTag(BFlatEncoding.Int16, "v");
      encoder.encode(value);
    }

    BFlatParser parser = new BFlatParser();

    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getInt16(), testData[index++]);
      assertEquals(value.getTag(), "v");
    }
  }
  @Test
  public void encodeInt32() throws Exception
  {
    final int testData[] = {0, -1, 1, 127, -128,
                            Integer.MIN_VALUE, Integer.MAX_VALUE};
    BFlatBuilder encoder = new BFlatBuilder(new byte[1024],0);
    for(int value : testData)
    {
      encoder.encodeTag(BFlatEncoding.Int32, "va");
      encoder.encode(value);
    }

    BFlatParser parser = new BFlatParser();

    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getInt32(), testData[index++]);
      assertEquals(value.getTag(), "va");
    }
  }
  @Test
  public void encodeInt64() throws Exception
  {
    final long testData[] = {0, -1, 1, 127, -128,
                             Long.MIN_VALUE, Long.MAX_VALUE};
    BFlatBuilder encoder = new BFlatBuilder(new byte[1024],0);
    for(long value : testData)
    {
      encoder.encodeTag(BFlatEncoding.Int64, "value");
      encoder.encode(value);
    }

    BFlatParser parser = new BFlatParser();

    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getInt64(), testData[index++]);
      assertEquals(value.getTag(), "value");
    }
  }
  @Test
  public void encodeDouble() throws Exception
  {
    final double testData[] = {0, -1.0, 1.0, 127.01, -128.01,
                               Double.MIN_VALUE, Double.MAX_VALUE };
    BFlatBuilder encoder = new BFlatBuilder(new byte[1024],0);
    for(double value : testData)
    {
      encoder.encodeTag(BFlatEncoding.Double, "value");
      encoder.encode(value);
    }

    BFlatParser parser = new BFlatParser();

    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getDouble(), testData[index++], 0.01);
      assertEquals(value.getTag(), "value");
    }
  }

  // Also tests varying tag lengths
  @Test
  public void encodeASCIIString() throws Exception
  {
    final int MAX_STRING = 16384;
    char[] tagData = new char[MAX_STRING];
    char[] data    = new char[MAX_STRING];

    Arrays.fill(tagData,'t');
    Arrays.fill(data, 'd');

    BFlatBuilder encoder =
      new BFlatBuilder(new byte[MAX_STRING*MAX_STRING*2],0);

    for(int length = 0; length < MAX_STRING; ++length)
    {
      encoder.encodeTag(BFlatEncoding.String, new String(tagData,0,length+1));
      encoder.encode(new String(data,0,length));
    }

    BFlatParser parser = new BFlatParser();
    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getTag(), new String(tagData,0,index+1));
      assertEquals(value.getString(), new String(data,0,index++));
    }
  }

  // Because unicode results in some alternate code paths exercised
  // for transcoding
  @Test
  public void encodeUnicodeString() throws Exception
  {
    final int MAX_STRING = 1024;
    char[] tagData = new char[MAX_STRING];
    char[] data    = new char[MAX_STRING];

    Arrays.fill(tagData,'ᄑ');
    Arrays.fill(data, '睷');

    BFlatBuilder encoder =
      new BFlatBuilder(new byte[MAX_STRING*MAX_STRING*4],0);

    for(int length = 0; length < MAX_STRING; ++length)
    {
      encoder.encodeTag(BFlatEncoding.String, new String(tagData,0,length+1));
      encoder.encode(new String(data,0,length));
    }

    BFlatParser parser = new BFlatParser();
    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getTag(), new String(tagData,0,index+1));
      assertEquals(value.getString(), new String(data,0,index++));
    }
  }
  @Test
  public void encodeDoubleArray() throws Exception
  {
    final double testData[] = {0, -1.0, 1.0, 127.01, -128.01,
                               Double.MIN_VALUE, Double.MAX_VALUE };
    BFlatBuilder encoder = new BFlatBuilder(new byte[1024],0);
    encoder.encodeTagArray(BFlatEncoding.Double, "value", testData.length);
    for(double value : testData)
    {
      encoder.encode(value);
    }

    BFlatParser parser = new BFlatParser();

    int index = 0;
    for(BFlatValue value : parser.parse(encoder.data,0,encoder.position))
    {
      assertEquals(value.getTag(), "value");
      assertEquals(testData.length, value.getArrayLength());
      for(int arrayIndex = 0; arrayIndex < value.getArrayLength(); ++arrayIndex)
      {
        assertEquals(testData[arrayIndex],value.getDouble(arrayIndex),0.1);
      }
    }
  }

  @Test
  public void encodeHelpers() throws Exception
  {
    BFlatBuilder encoder = new BFlatBuilder(new byte[1024],0);

    encoder.encode("tag1","data").encode("tag2",(byte)0)
           .encode("tag3",(short)10).encode("tag4",(int)20)
           .encode("tag5",(long)1000).encode("tag6",(double)1.0123);

    int index = 0;
    for(BFlatValue value : new BFlatParser().parse(encoder.data,0,encoder.position))
    {
      switch(index)
      {
        case 0:
          assertEquals(BFlatEncoding.String, value.getType());
          assertEquals("data", value.getString());
          assertEquals("tag1", value.getTag());
          break;
        case 1:
          assertEquals(BFlatEncoding.Int8, value.getType());
          assertEquals(0, value.getInt8());
          assertEquals("tag2", value.getTag());
          break;
        case 2:
          assertEquals(BFlatEncoding.Int16, value.getType());
          assertEquals(10, value.getInt16());
          assertEquals("tag3", value.getTag());
          break;
        case 3:
          assertEquals(BFlatEncoding.Int32, value.getType());
          assertEquals(20, value.getInt32());
          assertEquals("tag4", value.getTag());
          break;
        case 4:
          assertEquals(BFlatEncoding.Int64, value.getType());
          assertEquals(1000, value.getInt64());
          assertEquals("tag5", value.getTag());
          break;
        case 5:
          assertEquals(BFlatEncoding.Double, value.getType());
          assertEquals(1.0123, value.getDouble(),0.1);
          assertEquals("tag6", value.getTag());
          break;
        default:
          fail();
          break;
      }
      ++index;
    }
  }
}
