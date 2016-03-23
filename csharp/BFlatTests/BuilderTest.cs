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
using System.Linq;

namespace BFlatTests
{
    [TestClass]
    public class BuilderTest
    {
        [TestMethod]
        public void encodeInt8()
        {
            sbyte[] testData = { 0, -1, 1, 127, -128 };
            BFlatBuilder encoder = new BFlatBuilder(new byte[1024], 0);
            foreach (sbyte value in testData)
            {
                encoder.encodeTag(BFlatEncoding.Type.Int8, "value");
                encoder.encode(value);
            }

            BFlatParser parser = new BFlatParser();

            int index = 0;
            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getInt8(), testData[index++]);
                Assert.AreEqual(value.getTag(), "value");
            }
        }

        [TestMethod]
        public void encodeInt16()
        {
            short[] testData = { 0, -1, 1, 127, -128, 128, -129, -32768, 32767 };
            BFlatBuilder encoder = new BFlatBuilder(new byte[1024], 0);
            foreach (short value in testData)
            {
                encoder.encodeTag(BFlatEncoding.Type.Int16, "v");
                encoder.encode(value);
            }

            BFlatParser parser = new BFlatParser();

            int index = 0;
            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getInt16(), testData[index++]);
                Assert.AreEqual(value.getTag(), "v");
            }
        }
        [TestMethod]
        public void encodeInt32()
        {
            int[] testData = {
                0, -1, 1, 127, -128,
                                    Int32.MinValue, Int32.MaxValue};
            BFlatBuilder encoder = new BFlatBuilder(new byte[1024], 0);
            foreach (int value in testData)
            {
                encoder.encodeTag(BFlatEncoding.Type.Int32, "va");
                encoder.encode(value);
            }

            BFlatParser parser = new BFlatParser();

            int index = 0;
            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getInt32(), testData[index++]);
                Assert.AreEqual(value.getTag(), "va");
            }
        }

        [TestMethod]
        public void TestEncode64()
        {
            long[] testData = {
                0, -1, 1, 127, -128,
                                     Int64.MinValue,Int64.MaxValue};
            BFlatBuilder encoder = new BFlatBuilder(new byte[1024], 0);
            foreach (long value in testData)
            {
                encoder.encodeTag(BFlatEncoding.Type.Int64, "value");
                encoder.encode(value);
            }

            BFlatParser parser = new BFlatParser();

            int index = 0;
            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getInt64(), testData[index++]);
                Assert.AreEqual(value.getTag(), "value");
            }
        }

        [TestMethod]
        public void TestDouble()
        {
            double[] testData = {
        0, -1.0, 1.0, 127.01, -128.01,
                     Double.MinValue,Double.MaxValue };
            BFlatBuilder encoder = new BFlatBuilder(new byte[1024], 0);
            foreach (double value in testData)
            {
                encoder.encodeTag(BFlatEncoding.Type.Double, "value");
                encoder.encode(value);
            }

            BFlatParser parser = new BFlatParser();

            int index = 0;
            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getDouble(), testData[index++], 0.01);
                Assert.AreEqual(value.getTag(), "value");
            }
        }

        // Also tests varying tag lengths
        [TestMethod]
        public void encodeASCIIString()
        {
            int MAX_STRING = 16384;
            char[] tagData = Enumerable.Repeat('t', MAX_STRING).ToArray();
            char[] data = Enumerable.Repeat('d', MAX_STRING).ToArray();

            BFlatBuilder encoder =
                      new BFlatBuilder(new byte[MAX_STRING * MAX_STRING * 2], 0);

            for (int length = 0; length < MAX_STRING; ++length)
            {
                encoder.encodeTag(BFlatEncoding.Type.String, new String(tagData, 0, length + 1));
                encoder.encode(new String(data, 0, length));
            }

            BFlatParser parser = new BFlatParser();
            int index = 0;
            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getTag(), new String(tagData, 0, index + 1));
                Assert.AreEqual(value.getString(), new String(data, 0, index++));
            }
        }

        // Because unicode results in some alternate code paths exercised
        // for transcoding
        [TestMethod]
        public void encodeUnicodeString()
        {
            int MAX_STRING = 1024;

            char[] tagData = Enumerable.Repeat('㇃', MAX_STRING).ToArray();

            char[] data = Enumerable.Repeat('Ԁ', MAX_STRING).ToArray();
            BFlatBuilder encoder =
                      new BFlatBuilder(new byte[MAX_STRING * MAX_STRING * 4], 0);

            for (int length = 0; length < MAX_STRING; ++length)
            {
                encoder.encodeTag(BFlatEncoding.Type.String, new String(tagData, 0, length + 1));
                encoder.encode(new String(data, 0, length));
            }

            BFlatParser parser = new BFlatParser();
            int index = 0;
            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getTag(), new String(tagData, 0, index + 1));
                Assert.AreEqual(value.getString(), new String(data, 0, index++));
            }
        }

        [TestMethod]
        public void encodeDoubleArray()
        {
            double[] testData = {
                        0, -1.0, 1.0, 127.01, -128.01,
                                                       Double.MinValue, Double.MaxValue };
            BFlatBuilder encoder = new BFlatBuilder(new byte[1024], 0);
            encoder.encodeTagArray(BFlatEncoding.Type.Double, "value", testData.Length);
            foreach (double value in testData)
            {
                encoder.encode(value);
            }

            BFlatParser parser = new BFlatParser();

            foreach (BFlatValue value in parser.parse(encoder.data, 0, encoder.position))
            {
                Assert.AreEqual(value.getTag(), "value");
                Assert.AreEqual(testData.Length, value.getArrayLength());
                for (int arrayIndex = 0; arrayIndex < value.getArrayLength(); ++arrayIndex)
                {
                    Assert.AreEqual(testData[arrayIndex], value.getDouble(arrayIndex), 0.1);
                }
            }
        }

        [TestMethod]
        public void encodeHelpers()
        {
            BFlatBuilder encoder = new BFlatBuilder(new byte[1024], 0);

            encoder.encode("tag1", "data").encode("tag2", (sbyte)0)
                           .encode("tag3", (short)10).encode("tag4", (int)20)
                           .encode("tag5", (long)1000).encode("tag6", (double)1.0123);

            int index = 0;
            foreach (BFlatValue value in new BFlatParser().parse(encoder.data, 0, encoder.position))
            {
                switch (index)
                {
                    case 0:
                        Assert.AreEqual(BFlatEncoding.Type.String, value.getType());
                        Assert.AreEqual("data", value.getString());
                        Assert.AreEqual("tag1", value.getTag());
                        break;
                    case 1:
                        Assert.AreEqual(BFlatEncoding.Type.Int8, value.getType());
                        Assert.AreEqual(0, value.getInt8());
                        Assert.AreEqual("tag2", value.getTag());
                        break;
                    case 2:
                        Assert.AreEqual(BFlatEncoding.Type.Int16, value.getType());
                        Assert.AreEqual(10, value.getInt16());
                        Assert.AreEqual("tag3", value.getTag());
                        break;
                    case 3:
                        Assert.AreEqual(BFlatEncoding.Type.Int32, value.getType());
                        Assert.AreEqual(20, value.getInt32());
                        Assert.AreEqual("tag4", value.getTag());
                        break;
                    case 4:
                        Assert.AreEqual(BFlatEncoding.Type.Int64, value.getType());
                        Assert.AreEqual(1000, value.getInt64());
                        Assert.AreEqual("tag5", value.getTag());
                        break;
                    case 5:
                        Assert.AreEqual(BFlatEncoding.Type.Double, value.getType());
                        Assert.AreEqual(1.0123, value.getDouble(), 0.1);
                        Assert.AreEqual("tag6", value.getTag());
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
                ++index;
            }
        }

    }

}
