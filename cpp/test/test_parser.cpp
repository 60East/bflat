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
#include "gtest/gtest.h"
#include <bflat/bflat.hpp>


TEST(ParserTest, scalarInt32)
{
  const char data[] = { 0x2b, 0x66, 0x6f, 0x6f, 0x1, 0x0, 0x0, 0x0 };
  const size_t length = sizeof(data);

  bflat::bflat_deserializer parser;
  parser.reset(data, length);

  int64_t int_value;
  bflat::bflat_value value;
  ASSERT_EQ(true, parser.getNext(value));
  ASSERT_EQ(bflat::int32_type, value.valueType());
  ASSERT_EQ(0, value.getInt(int_value));
  ASSERT_EQ(1, int_value);
  ASSERT_EQ("foo", value.tagString().asString());
  ASSERT_FALSE(parser.getNext(value));
}

TEST(ParserTest, longTagName)
{
  const char data[] = {0x28, 0x19, 0x74, 0x68, 0x69, 0x73, 0x20, 0x69, 0x73, 0x20
    , 0x61, 0x20, 0x6C, 0x6F, 0x6E, 0x67, 0x65, 0x72, 0x20, 0x74
      , 0x61, 0x67, 0x20, 0x6E, 0x61, 0x6D, 0x65, 0x1, 0x0, 0x0
      , 0x0};

  bflat::bflat_deserializer parser;
  parser.reset(data, sizeof(data));

  bflat::bflat_value value;
  int64_t int_value;
  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ("this is a longer tag name", value.tagString().asString());
  ASSERT_EQ(0, value.getInt(int_value));
  ASSERT_EQ(1, int_value);
}

TEST(ParserTest, scalarDoubleStringDouble)
{
  // BFLAT encoding of (101, {'double': 123.45, 'long string tag': 'the quick brown fox jumped over the lazy dog', 'another double': -123.99})
  const unsigned char data[] = {
  0x3E, 0x64, 0x6F, 0x75, 0x62, 0x6C, 0x65, 0xCD, 0xCC, 0xCC, 
  0xCC, 0xCC, 0xDC, 0x5E, 0x40, 0x8, 0xF, 0x6C, 0x6F, 0x6E, 
  0x67, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x20, 0x74, 
  0x61, 0x67, 0x2C, 0x74, 0x68, 0x65, 0x20, 0x71, 0x75, 0x69, 
  0x63, 0x6B, 0x20, 0x62, 0x72, 0x6F, 0x77, 0x6E, 0x20, 0x66, 
  0x6F, 0x78, 0x20, 0x6A, 0x75, 0x6D, 0x70, 0x65, 0x64, 0x20, 
  0x6F, 0x76, 0x65, 0x72, 0x20, 0x74, 0x68, 0x65, 0x20, 0x6C, 
  0x61, 0x7A, 0x79, 0x20, 0x64, 0x6F, 0x67, 0x38, 0xE, 0x61, 
  0x6E, 0x6F, 0x74, 0x68, 0x65, 0x72, 0x20, 0x64, 0x6F, 0x75, 
  0x62, 0x6C, 0x65, 0x8F, 0xC2, 0xF5, 0x28, 0x5C, 0xFF, 0x5E, 
  0xC0};

  bflat::bflat_deserializer parser;
  parser.reset((const char*)data, sizeof(data));

  bflat::bflat_value value;
  int64_t int_value;
  bflat::string_value string_value;
  double double_value;

  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ(0, value.getDouble(double_value));
  ASSERT_EQ("double", value.tagString().asString());
  ASSERT_EQ(123.45, double_value);

  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ(0, value.getString(string_value));
  ASSERT_EQ("the quick brown fox jumped over the lazy dog", string_value.asString());
  ASSERT_EQ("long string tag", value.tagString().asString());

  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ(0, value.getDouble(double_value));
  ASSERT_EQ("another double", value.tagString().asString());
  ASSERT_EQ(-123.99, double_value);

  ASSERT_FALSE(parser.getNext(value));
}

TEST(ParserTest, leb128Array)
{
  const int64_t expected[]= { 0, -1, 1, -127, 127, -128, 128, -65536, 65536 };
  const unsigned char data[] = {
  0xCE, 0x6C, 0x65, 0x62, 0x31, 0x32, 0x38, 0x9, 0x0, 0x7F,
  0x1, 0x81, 0x7F, 0xFF, 0x0, 0x80, 0x7F, 0x80, 0x1, 0x80,
  0x80, 0x7C, 0x80, 0x80, 0x4};
  bflat::bflat_deserializer parser;
  parser.reset((const char*)data, sizeof(data));
  bflat::bflat_value value;

  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ(bflat::leb128_type, value.valueType());
  ASSERT_TRUE(value.isArray());
  ASSERT_EQ(9, value.length());

  for(size_t index = 0; index < value.length(); ++index)
  {
    int64_t value;
    ASSERT_TRUE(parser.decode_array_leb128(value));
    ASSERT_EQ(expected[index], value);
  }
}

TEST(ParserTest, binaryArray)
{
  const std::string expected[] = { "", "a", "aaa", "aaaa", "a", "" };
  const unsigned char data[] = {
0x96, 0x62, 0x69, 0x6E, 0x61, 0x72, 0x79, 0x6, 0x0, 0x1,0x61, 0x3, 0x61, 0x61, 0x61, 0x4, 0x61, 0x61, 0x61, 0x61, 0x1, 0x61, 0x0, 0x2b, 0x66, 0x6f, 0x6f, 0x1, 0x0, 0x0, 0x0};
  bflat::bflat_deserializer parser;
  parser.reset((const char*)data, sizeof(data));
  bflat::bflat_value value;
  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ(bflat::binary_type, value.valueType());
  ASSERT_TRUE(value.isArray());
  ASSERT_EQ(6, value.length());

  for(size_t index = 0; index < value.length(); ++index)
  {
    bflat::string_value string_value;
    ASSERT_TRUE(parser.decode_array_binary(string_value));
    ASSERT_EQ(expected[index], string_value.asString());
  }
  ASSERT_TRUE(parser.getNext(value));
  int64_t int_value;
  ASSERT_EQ(0, value.getInt(int_value));
  ASSERT_EQ(1,int_value);
  ASSERT_EQ("foo",value.tagString().asString());

  ASSERT_FALSE(parser.getNext(value));
}

TEST(ParserTest, nullStringDouble)
{
  const unsigned char data[] = {
  0x4, 0x6E, 0x75, 0x6C, 0x6C, 0x8, 0x10, 0x73, 0x74, 0x72,
  0x69, 0x6E, 0x67, 0x20, 0x67, 0x6F, 0x65, 0x73, 0x20, 0x68,
  0x65, 0x72, 0x65, 0x1, 0x61, 0x3E, 0x64, 0x6F, 0x75, 0x62,
  0x6C, 0x65, 0x73, 0x68, 0x91, 0xED, 0x7C, 0xFF, 0x23, 0x40};

  bflat::bflat_deserializer parser;
  parser.reset((const char*) data, sizeof(data));
  bflat::bflat_value value;
  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ(bflat::null_type, value.valueType());
  ASSERT_TRUE(value.isNull());

  bflat::string_value string_value;
  ASSERT_TRUE(parser.getNext(value));
  ASSERT_EQ(bflat::string_type, value.valueType());
  ASSERT_EQ(0, value.getString(string_value));
  ASSERT_EQ("a", string_value.asString());
  ASSERT_EQ("string goes here", value.tagString().asString());

  ASSERT_TRUE(parser.getNext(value));
  double double_value;
  ASSERT_EQ(0, value.getDouble(double_value));
  ASSERT_EQ(9.999, double_value);

  ASSERT_FALSE(parser.getNext(value));
}



