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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BFlat;

namespace BFLatTests
{
    [TestClass]
    public class FixedTest
    {
        [TestMethod]
        public void TestInt8()
        {
            byte[] buf = new byte[1];
            sbyte data = 0;
            Assert.AreEqual(1, Fixed.encodeInt8(data, buf, 0));
            Assert.AreEqual(data, Fixed.decodeInt8(buf, 0));

            data = 1;
            Fixed.encodeInt8(data, buf, 0);
            Assert.AreEqual(data, Fixed.decodeInt8(buf, 0));

            data = -128;
            Fixed.encodeInt8(data, buf, 0);
            Assert.AreEqual(data, Fixed.decodeInt8(buf, 0));
        }

        [TestMethod]
        public void TestInt16()
        {
            byte[] buf = new byte[2];
            short data = 0;

            Assert.AreEqual(2, Fixed.encodeInt16(data, buf, 0));
            Assert.AreEqual(data, Fixed.decodeInt16(buf, 0));

            data = 2;
            Assert.AreEqual(2, Fixed.encodeInt16(data, buf, 0));
            Assert.AreEqual(2, buf[0]);
            Assert.AreEqual(0, buf[1]);
            Assert.AreEqual(data, Fixed.decodeInt16(buf, 0));

            data = -2;
            Assert.AreEqual(2, Fixed.encodeInt16(data, buf, 0));
            Assert.AreEqual((byte)0xfe, buf[0]);
            Assert.AreEqual((byte)0xff, buf[1]);
            Assert.AreEqual(data, Fixed.decodeInt16(buf, 0));

            data = -32768;
            Assert.AreEqual(2, Fixed.encodeInt16(data, buf, 0));
            Assert.AreEqual((byte)0x00, buf[0]);
            Assert.AreEqual((byte)0x80, buf[1]);
            Assert.AreEqual(data, Fixed.decodeInt16(buf, 0));
        }
        [TestMethod]
        public void TestInt32()
        {
            byte[] buf = new byte[4];
            int data = 0;
            Assert.AreEqual(4, Fixed.encodeInt32(data, buf, 0));
            Assert.AreEqual(data, Fixed.decodeInt32(buf, 0));

            data = -1;
            Assert.AreEqual(4, Fixed.encodeInt32(data, buf, 0));
            Assert.AreEqual(data, Fixed.decodeInt32(buf, 0));
            data = Int32.MinValue;
            Assert.AreEqual(4, Fixed.encodeInt32(data, buf, 0));
            Assert.AreEqual(data, Fixed.decodeInt32(buf, 0));
            data = Int32.MaxValue;
            Assert.AreEqual(4, Fixed.encodeInt32(data, buf, 0));
            Assert.AreEqual(data, Fixed.decodeInt32(buf, 0));

        }
        [TestMethod]
        public void TestInt64()
        {
            byte[] buf = new byte[8];
            long data = 0xfffefdfcfbfaf9L;

            Assert.AreEqual(8, Fixed.encodeInt64(data, buf, 0));
            Assert.AreEqual((byte)0xf9, buf[0]);
            Assert.AreEqual((byte)0xfa, buf[1]);
            Assert.AreEqual((byte)0xfb, buf[2]);
            Assert.AreEqual((byte)0xfc, buf[3]);
            Assert.AreEqual((byte)0xfd, buf[4]);
            Assert.AreEqual((byte)0xfe, buf[5]);
            Assert.AreEqual((byte)0xff, buf[6]);
            Assert.AreEqual((byte)0x00, buf[7]);
            Assert.AreEqual(data, Fixed.decodeInt64(buf, 0));

            data *= -1;
            Assert.AreEqual(8, Fixed.encodeInt64(data, buf, 0));
            Assert.AreEqual((byte)0x07, buf[0]);
            Assert.AreEqual((byte)0x05, buf[1]);
            Assert.AreEqual((byte)0x04, buf[2]);
            Assert.AreEqual((byte)0x03, buf[3]);
            Assert.AreEqual((byte)0x02, buf[4]);
            Assert.AreEqual((byte)0x01, buf[5]);
            Assert.AreEqual((byte)0x00, buf[6]);
            Assert.AreEqual((byte)0xff, buf[7]);
            Assert.AreEqual(data, Fixed.decodeInt64(buf, 0));
        }

        [TestMethod]
        public void TestDouble()
        {
            byte[] buf = new byte[8];
            double data = -1.23;

            Assert.AreEqual(8, Fixed.encodeDouble(data, buf, 0));
            Assert.AreEqual((byte)0xae, buf[0]);
            Assert.AreEqual((byte)'G', buf[1]);
            Assert.AreEqual((byte)0xe1, buf[2]);
            Assert.AreEqual((byte)'z', buf[3]);
            Assert.AreEqual((byte)0x14, buf[4]);
            Assert.AreEqual((byte)0xae, buf[5]);
            Assert.AreEqual((byte)0xf3, buf[6]);
            Assert.AreEqual((byte)0xbf, buf[7]);
            Assert.AreEqual(data, Fixed.decodeDouble(buf, 0), 0.01);

            data *= -20;
            Assert.AreEqual(8, Fixed.encodeDouble(data, buf, 0));
            Assert.AreEqual(data, Fixed.decodeDouble(buf, 0), 0.01);
        }
    }
}
