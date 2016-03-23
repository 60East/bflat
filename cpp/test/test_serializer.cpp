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

TEST(test_serializer, encodeInt8)
{
  typedef int8_t test_type;

  const test_type testData[] = {0, -1, 1, 127, -128, INT8_MIN, INT8_MAX};

  bflat::bflat_serializer serializer;

  for(auto value : testData)
  {
    serializer.append_int8("value",5,value);
  }

  bflat::bflat_value value;
  bflat::bflat_deserializer parser;
  parser.reset(serializer.data(),serializer.length());

  size_t index = 0;
  while(parser.getNext(value))
  {
    int64_t int_value;
    ASSERT_EQ(bflat::int8_type, value.valueType());
    ASSERT_EQ(0, value.getInt(int_value));
    ASSERT_EQ(testData[index++], int_value);
    ASSERT_EQ("value", value.tagString().asString());
  }
  ASSERT_EQ(sizeof(testData)/sizeof(test_type), index);
}

TEST(test_serializer, encodeInt16)
{
  typedef int16_t test_type;

  const test_type testData[] = {0, -1, 1, 127, -128, INT16_MIN, INT16_MAX};

  bflat::bflat_serializer serializer;

  for(auto value : testData)
  {
    serializer.append_int16("value",5,value);
  }

  bflat::bflat_value value;
  bflat::bflat_deserializer parser;
  parser.reset(serializer.data(),serializer.length());

  size_t index = 0;
  while(parser.getNext(value))
  {
    int64_t int_value;
    ASSERT_EQ(bflat::int16_type, value.valueType());
    ASSERT_EQ(0, value.getInt(int_value));
    ASSERT_EQ(testData[index++], int_value);
  }
  ASSERT_EQ(sizeof(testData)/sizeof(test_type), index);
}


TEST(test_serializer, encodeInt32)
{
  typedef int32_t test_type;

  const test_type testData[] = {0, -1, 1, 127, -128, INT32_MIN, INT32_MAX};

  bflat::bflat_serializer serializer;

  for(auto value : testData)
  {
    serializer.append_int32("value",5,value);
  }

  bflat::bflat_value value;
  bflat::bflat_deserializer parser;
  parser.reset(serializer.data(),serializer.length());

  size_t index = 0;
  while(parser.getNext(value))
  {
    int64_t int_value;
    ASSERT_EQ(bflat::int32_type, value.valueType());
    ASSERT_EQ(0, value.getInt(int_value));
    ASSERT_EQ(testData[index++], int_value);
  }
  ASSERT_EQ(sizeof(testData)/sizeof(test_type), index);
}


TEST(test_serializer, encodeInt64)
{
  typedef int64_t test_type;

  const test_type testData[] = {0, -1, 1, 127, -128, INT64_MIN, INT64_MAX};

  bflat::bflat_serializer serializer;

  for(auto value : testData)
  {
    serializer.append_int64("value",5,value);
  }

  bflat::bflat_value value;
  bflat::bflat_deserializer parser;
  parser.reset(serializer.data(),serializer.length());

  size_t index = 0;
  while(parser.getNext(value))
  {
    int64_t int_value;
    ASSERT_EQ(bflat::int64_type, value.valueType());
    ASSERT_EQ(0, value.getInt(int_value));
    ASSERT_EQ(testData[index++], int_value);
  }
  ASSERT_EQ(sizeof(testData)/sizeof(test_type), index);
}

TEST(test_serializer, encodeDouble)
{
  typedef double test_type;

  const test_type testData[] = {0, -1.0, 1.0, 127.01, -128.01, DBL_MIN, DBL_MAX};

  bflat::bflat_serializer serializer;

  for(auto value : testData)
  {
    serializer.append_double("value",5,value);
  }

  bflat::bflat_value value;
  bflat::bflat_deserializer parser;
  parser.reset(serializer.data(),serializer.length());

  size_t index = 0;
  while(parser.getNext(value))
  {
    double dbl_value;
    ASSERT_EQ(bflat::double_type, value.valueType());
    ASSERT_EQ(0, value.getDouble(dbl_value));
    ASSERT_EQ(testData[index++], dbl_value);
  }
  ASSERT_EQ(sizeof(testData)/sizeof(test_type), index);
}

TEST(test_serializer, encodeLeb128)
{
  typedef int64_t test_type;

  const test_type testData[] = {0, -1, 1, 127, -128, INT64_MIN, INT64_MAX};

  bflat::bflat_serializer serializer;

  for(auto value : testData)
  {
    serializer.append_leb128("value",5,value);
  }

  bflat::bflat_value value;
  bflat::bflat_deserializer parser;
  parser.reset(serializer.data(),serializer.length());

  size_t index = 0;
  while(parser.getNext(value))
  {
    int64_t int_value;
    ASSERT_EQ(bflat::leb128_type, value.valueType());
    ASSERT_EQ(0, value.getInt(int_value));
    ASSERT_EQ(testData[index++], int_value);
  }
  ASSERT_EQ(sizeof(testData)/sizeof(test_type), index);
}

