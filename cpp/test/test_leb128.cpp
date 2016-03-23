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

TEST(Leb128, testSleb128Interop)
{
  unsigned char buffer[128];
  int64_t decoded_value;
  // Test well known values
  ASSERT_EQ(1, bflat::encode_sleb_128(1, buffer));
  ASSERT_EQ(1, buffer[0]);
  ASSERT_EQ(1, bflat::decode_sleb_128(buffer, 1, decoded_value));
  ASSERT_EQ(1, decoded_value);

  ASSERT_EQ(1, bflat::encode_sleb_128(0, buffer));
  ASSERT_EQ(0, buffer[0]);
  ASSERT_EQ(1, bflat::decode_sleb_128(buffer, 1, decoded_value));
  ASSERT_EQ(0, decoded_value);

  ASSERT_EQ(1, bflat::encode_sleb_128(-1, buffer));
  ASSERT_EQ(0x7f, buffer[0]);
  ASSERT_EQ(1, bflat::decode_sleb_128(buffer, 1, decoded_value));
  ASSERT_EQ(-1, decoded_value);

  ASSERT_EQ(2, bflat::encode_sleb_128(127, buffer));
  ASSERT_EQ(0xff, buffer[0]);
  ASSERT_EQ(0x00, buffer[1]);
  ASSERT_EQ(2, bflat::decode_sleb_128(buffer, 2, decoded_value));
  ASSERT_EQ(127, decoded_value);

  ASSERT_EQ(2, bflat::encode_sleb_128(-128, buffer));
  ASSERT_EQ(0x80, buffer[0]);
  ASSERT_EQ(0x7f, buffer[1]);
  ASSERT_EQ(2, bflat::decode_sleb_128(buffer, 2, decoded_value));
  ASSERT_EQ(-128, decoded_value);
}

TEST(Leb128, testSleb128EncodeDecode)
{
  const int64_t testcases[] = { 0, -1, -63, -64, -127, -128, -65535, -65536,
                          1, 2, 63, 64, 127, 128, 255, 256, 32767, 32768,
                          65535, 65536, INT64_MIN, INT64_MAX};
  const size_t entries = sizeof(testcases) / sizeof(int64_t);
  unsigned char buffer[128];
  unsigned char *dst = buffer;

  for(int64_t value : testcases)
  {
    dst += bflat::encode_sleb_128(value, dst);
  }

  unsigned char *src = buffer;
  size_t index = 0;
  while(dst != src)
  {
    int64_t value = 0;
    int rc = bflat::decode_sleb_128(src, dst - src, value);
    ASSERT_EQ(testcases[index], value);
    int rc2 = bflat::decode_sleb_128(src, value);
    ASSERT_EQ(testcases[index], value);
    ASSERT_EQ(rc, rc2);
    ASSERT_LT(0, rc);
    src += rc;

    ++index;
  }

  ASSERT_EQ(index, entries);
}

TEST(Leb128, testUleb128Interop)
{
  unsigned char buffer[128];
  uint64_t decoded_value;
  ASSERT_EQ(1, bflat::encode_uleb_128(0, buffer));
  ASSERT_EQ(0, buffer[0]);
  ASSERT_EQ(1, bflat::decode_uleb_128(buffer,1,decoded_value));
  ASSERT_EQ(0, decoded_value);

  ASSERT_EQ(1, bflat::encode_uleb_128(1, buffer));
  ASSERT_EQ(1, buffer[0]);
  ASSERT_EQ(1, bflat::decode_uleb_128(buffer,1,decoded_value));
  ASSERT_EQ(1, decoded_value);

  ASSERT_EQ(1, bflat::encode_uleb_128(127, buffer));
  ASSERT_EQ(0x7f, buffer[0]);
  ASSERT_EQ(1, bflat::decode_uleb_128(buffer,1,decoded_value));
  ASSERT_EQ(127, decoded_value);

  ASSERT_EQ(2, bflat::encode_uleb_128(128, buffer));
  ASSERT_EQ(0x80, buffer[0]);
  ASSERT_EQ(0x01, buffer[1]);
  ASSERT_EQ(2, bflat::decode_uleb_128(buffer,2,decoded_value));
  ASSERT_EQ(128, decoded_value);
}

TEST(Leb128, testUleb128EncodeDecode)
{
  const uint64_t testcases[] = { 0, 1, 2, 63, 64, 127, 128, 255, 256, 32767, 32768,
                          65535, 65536, UINT64_MAX };
  const size_t entries = sizeof(testcases) / sizeof(int64_t);
  unsigned char buffer[128];
  unsigned char *dst = buffer;

  for(uint64_t value : testcases)
  {
    dst += bflat::encode_uleb_128(value, dst);
  }

  unsigned char *src = buffer;
  size_t index = 0;
  while(dst != src)
  {
    uint64_t value = 0;
    src += bflat::decode_uleb_128(src, dst - src, value);
    ASSERT_EQ(testcases[index], value);
    ++index;
  }

  ASSERT_EQ(index, entries);
}

