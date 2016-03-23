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

namespace BFlatTests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void scalarInt32()
        {
            // BFLAT encoding of {"foo":1} (int32)
            byte[] data = { 0x2B, 0x66, 0x6F, 0x6F, 0x1, 0x0, 0x0, 0x0 };

            BFlatParser p = new BFlatParser().parse(data);
            Assert.IsTrue(p.MoveNext());
            BFlatValue v = p.Current;
            Assert.IsNotNull(v);
            Assert.AreEqual(BFlatEncoding.Type.Int32, v.getType());
            Assert.IsFalse(v.isArray());
            Assert.AreEqual("foo", v.getTag());
            Assert.AreEqual(1, v.getLong());
            Assert.AreEqual(1, v.getInt32());
            Assert.IsFalse(v.isNull());
            Assert.AreEqual(v.ToString(), "foo=1");
            Assert.IsFalse(p.MoveNext());
        }

        [TestMethod]
        public void longTagName()
        {
            // BFLAT encoding of {"this is a longer tag name":1}
            byte[] data = {
            0x28, 0x19, 0x74, 0x68, 0x69, 0x73, 0x20, 0x69, 0x73, 0x20
    , 0x61, 0x20, 0x6C, 0x6F, 0x6E, 0x67, 0x65, 0x72, 0x20, 0x74
    , 0x61, 0x67, 0x20, 0x6E, 0x61, 0x6D, 0x65, 0x1, 0x0, 0x0
    , 0x0};

            BFlatParser p = new BFlatParser().parse(data);
            Assert.IsTrue(p.MoveNext());
            Assert.AreEqual(p.Current.ToString(), "this is a longer tag name=1");
        }
        [TestMethod]
        public void scalarDoubleStringDouble()
        {
            // BFLAT encoding of (101, {'double': 123.45, 'long string tag': 'the quick brown fox jumped over the lazy dog', 'another double': -123.99})
            byte[] data = {
                    (byte)0x3E, (byte)0x64, (byte)0x6F, (byte)0x75, (byte)0x62, (byte)0x6C, (byte)0x65, (byte)0xCD, (byte)0xCC, (byte)0xCC,
                    (byte)0xCC, (byte)0xCC, (byte)0xDC, (byte)0x5E, (byte)0x40, (byte)0x8, (byte)0xF, (byte)0x6C, (byte)0x6F, (byte)0x6E,
                    (byte)0x67, (byte)0x20, (byte)0x73, (byte)0x74, (byte)0x72, (byte)0x69, (byte)0x6E, (byte)0x67, (byte)0x20, (byte)0x74,
                    (byte)0x61, (byte)0x67, (byte)0x2C, (byte)0x74, (byte)0x68, (byte)0x65, (byte)0x20, (byte)0x71, (byte)0x75, (byte)0x69,
                    (byte)0x63, (byte)0x6B, (byte)0x20, (byte)0x62, (byte)0x72, (byte)0x6F, (byte)0x77, (byte)0x6E, (byte)0x20, (byte)0x66,
                    (byte)0x6F, (byte)0x78, (byte)0x20, (byte)0x6A, (byte)0x75, (byte)0x6D, (byte)0x70, (byte)0x65, (byte)0x64, (byte)0x20,
                    (byte)0x6F, (byte)0x76, (byte)0x65, (byte)0x72, (byte)0x20, (byte)0x74, (byte)0x68, (byte)0x65, (byte)0x20, (byte)0x6C,
                    (byte)0x61, (byte)0x7A, (byte)0x79, (byte)0x20, (byte)0x64, (byte)0x6F, (byte)0x67, (byte)0x38, (byte)0xE, (byte)0x61,
                    (byte)0x6E, (byte)0x6F, (byte)0x74, (byte)0x68, (byte)0x65, (byte)0x72, (byte)0x20, (byte)0x64, (byte)0x6F, (byte)0x75,
                    (byte)0x62, (byte)0x6C, (byte)0x65, (byte)0x8F, (byte)0xC2, (byte)0xF5, (byte)0x28, (byte)0x5C, (byte)0xFF, (byte)0x5E,
                    (byte)0xC0};

            BFlatParser p = new BFlatParser().parse(data);
            Assert.IsTrue(p.MoveNext());
            Assert.AreEqual(p.Current.ToString(), "double=123.45");
            p.MoveNext();
            Assert.AreEqual(p.Current.ToString(), "long string tag=\"the quick brown fox jumped over the lazy dog\"");
            p.MoveNext();
            Assert.AreEqual(p.Current.ToString(), "another double=-123.99");
        }
        [TestMethod]
        public void leb128Array()
        {
            byte[] data = {
                (byte)0xCE, (byte)0x6C, (byte)0x65, (byte)0x62, (byte)0x31, (byte)0x32, (byte)0x38, (byte)0x9, (byte)0x0, (byte)0x7F,
                (byte)0x1, (byte)0x81, (byte)0x7F, (byte)0xFF, (byte)0x0, (byte)0x80, (byte)0x7F, (byte)0x80, (byte)0x1, (byte)0x80,
                (byte)0x80, (byte)0x7C, (byte)0x80, (byte)0x80, (byte)0x4};
            BFlatParser p = new BFlatParser().parse(data);
            Assert.IsTrue(p.MoveNext());
            Assert.AreEqual(p.Current.ToString(), "leb128=[0, -1, 1, -127, 127, -128, 128, -65536, 65536]");
        }


        [TestMethod]
        public void binaryArray()
        {
            byte[] data = {
            (byte)0x96, (byte)0x62, (byte)0x69, (byte)0x6E, (byte)0x61, (byte)0x72, (byte)0x79, (byte)0x6, (byte)0x0, (byte)0x1,
            (byte)0x61, (byte)0x3, (byte)0x61, (byte)0x61, (byte)0x61, (byte)0x4, (byte)0x61, (byte)0x61, (byte)0x61, (byte)0x61,
            (byte)0x1, (byte)0x61, (byte)0x0};
            BFlatParser p = new BFlatParser().parse(data);
            Assert.IsTrue(p.MoveNext());
            Assert.AreEqual(p.Current.ToString(), "binary=[\"\", \"a\", \"aaa\", \"aaaa\", \"a\", \"\"]");
        }

        [TestMethod]
        public void nullStringDouble()
        {
            byte[] data = {
            (byte)0x4, (byte)0x6E, (byte)0x75, (byte)0x6C, (byte)0x6C, (byte)0x8, (byte)0x10, (byte)0x73, (byte)0x74, (byte)0x72,
    (byte)0x69, (byte)0x6E, (byte)0x67, (byte)0x20, (byte)0x67, (byte)0x6F, (byte)0x65, (byte)0x73, (byte)0x20, (byte)0x68,
    (byte)0x65, (byte)0x72, (byte)0x65, (byte)0x1, (byte)0x61, (byte)0x3E, (byte)0x64, (byte)0x6F, (byte)0x75, (byte)0x62,
    (byte)0x6C, (byte)0x65, (byte)0x73, (byte)0x68, (byte)0x91, (byte)0xED, (byte)0x7C, (byte)0xFF, (byte)0x23, (byte)0x40};

            BFlatParser p = new BFlatParser().parse(data);
            Assert.IsTrue(p.MoveNext());
            Assert.AreEqual("null=null", p.Current.ToString());
            p.MoveNext();
            Assert.AreEqual("string goes here=\"a\"", p.Current.ToString());
            p.MoveNext();
            Assert.AreEqual("double=9.999", p.Current.ToString());
            Assert.IsFalse(p.MoveNext());
        }

        [TestMethod]
        public void nullStringDoubleReuse()
        {
            byte[] data = {
            (byte)0x4, (byte)0x6E, (byte)0x75, (byte)0x6C, (byte)0x6C, (byte)0x8, (byte)0x10, (byte)0x73, (byte)0x74, (byte)0x72,
            (byte)0x69, (byte)0x6E, (byte)0x67, (byte)0x20, (byte)0x67, (byte)0x6F, (byte)0x65, (byte)0x73, (byte)0x20, (byte)0x68,
            (byte)0x65, (byte)0x72, (byte)0x65, (byte)0x1, (byte)0x61, (byte)0x3E, (byte)0x64, (byte)0x6F, (byte)0x75, (byte)0x62,
            (byte)0x6C, (byte)0x65, (byte)0x73, (byte)0x68, (byte)0x91, (byte)0xED, (byte)0x7C, (byte)0xFF, (byte)0x23, (byte)0x40};

            String[] expected = { "null=null", "string goes here=\"a\"", "double=9.999" };
            BFlatParser p = new BFlatParser().parse(data);
            int i = 0;
            BFlatValue prev = null;
            foreach (BFlatValue v in p)
            {
                if (prev != null)
                    Assert.IsTrue(Object.ReferenceEquals(v, prev));
                Assert.AreEqual(expected[i], v.ToString());
                ++i;
                v.reuse();
                prev = v;
            }

            p.parse(data);
            i = 0;
            prev = null;
            foreach (BFlatValue v in p)
            {
                Assert.IsFalse(Object.ReferenceEquals(v, prev));
                Assert.AreEqual(expected[i], v.ToString());
                ++i;
                prev = v;
            }
        }
    }

}
