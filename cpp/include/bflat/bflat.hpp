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
#ifndef _BFLAT_BFLAT_HPP_
#define _BFLAT_BFLAT_HPP_

/** \file  bflat/bflat.hpp
 *  \brief The header file containing functions and classes to parse and
 *         encode BFlat messages. The BFlat format is described at
 *         http://bflat.io .
 */

#include <cstdint>
#include <cmath>
#include <string.h>
#include <string>

namespace bflat
{
  static const int    format_error = -1;
  static const int    tag_error    = -2;
  static const double version      = 1.02;

  /// \brief The allowed value type constants for BFlat value types.
  enum value_type : uint8_t
  {
    /// \brief The type constant for a NULL value.
    null_type     = 0x0,

    /// \brief The type constant for a UTF-8 string.
    string_type   = 0x1,

    /// \brief The type constant for variable-length binary data.
    binary_type   = 0x2,

    /// \brief The type constant for a signed 8-bit integer.
    int8_type     = 0x3,

    /// \brief The type constant for a signed 16-bit integer.
    int16_type    = 0x4,

    /// \brief The type constant for a signed 32-bit integer.
    int32_type    = 0x5,

    /// \brief The type constant for a signed 64-bit integer.
    int64_type    = 0x6,

    /// \brief The type constant for a double-precision floating point number.
    double_type   = 0x7,

    /// \brief The type constant for a datetime, encoded as a 64-bit integer.
    datetime_type = 0x8,

    /// \brief The type constant for a signed integer, up to 128 bits, encoded
    ///        with the LEB128 variable length encoding.
    leb128_type   = 0x9
  };

  /// \brief The type constant indicating this value is an array.
  ///
  ///        This value is bitwise OR'ed with one of the other types to
  ///        indicate the type of the values in the array.
  static const uint8_t array_type    = 0x10;

  // Used to check if a particular type is an array.
  static const uint8_t is_array_mask = 0x80;

  // Used to bitwise-AND away the array bits.
  static const uint8_t type_mask       = 0xF;

  // Used to bitwise-AND away the length of the tag, when the
  //   tag name is short.
  static const uint8_t tag_length_mask = 0x7;
  static const uint8_t type_shift      = 3;

  static const uint8_t min_length      = 2; // a single char tag with null value

  // Used for LEB128 code.
  static const uint8_t leb_128_value_mask       = 0x7F;
  static const uint8_t leb_128_stop_bit_mask    = 0x80;
  static const uint8_t leb_128_sign_bit_mask    = 0x40;
  static const uint8_t leb_128_null_value       = 0x00;
  static const uint8_t leb_128_value_bit_shift  = 7;

  /// \brief Represents a UTF-8 encoded string found in the source data.
  ///
  /// Wraps a pointer and a length for a string found in the source data.
  /// Use the data() and length() methods to retrieve the location in your
  /// source data, or the asString() method to convert to std::string.
  struct string_value
  {
    /// \brief Constructs string_value with a NULL pointer and zero length.
    string_value(void)
      : _p(0),
        _len(0)
    {
      ;
    }

    /// \brief   Returns the UTF-8 bytes of this string.
    /// \returns A pointer into the parsed data where this string begins.
    /// \warning This string is not null-terminated; use length()
    ///          to determine this string's length. Passing this pointer
    ///          to functions requiring a null-terminated string may result
    ///          in undefined behavior.
    inline const char *data(void) const
    {
      return _p;
    }

    /// \brief Returns the length of this string in bytes.
    /// \returns The length of this string, in bytes.
    inline size_t length(void) const
    {
      return _len;
    }

    /// \brief Returns this string as a std::string.
    ///
    /// This method creates a new std::string with a copy of the bytes of
    /// this string. Use this method when working with APIs that require
    /// an std::string, or those that require a null-terminated string.
    /// \returns A new std::string containing a copy of this string's data.
    std::string asString(void) const
    {
      if(_p && _len)
        return std::string(_p,_len);
      return std::string();
    }

    const char *_p;
    size_t      _len;
  };

  inline size_t upper_power_of_two(size_t value_)
  {
    value_--;
    value_ |= value_ >> 1;
    value_ |= value_ >> 2;
    value_ |= value_ >> 4;
    value_ |= value_ >> 8;
    value_ |= value_ >> 16;
    value_++;
    return value_;
  }

  inline uint8_t make_array_type(uint8_t valueType_)
  {
    return valueType_ | array_type;
  }

  inline size_t get_max_uleb_128_size(uint64_t value_)
  {
    return ((8 * sizeof (value_) + 6) / 7);
  }

  inline size_t get_max_sleb_128_size(int64_t value_)
  {
    return ((8 * sizeof(value_) + 6) / 7);
  }

  inline size_t get_uleb_128_size(uint64_t value_)
  {
    size_t size = 0;
    while (value_)
    {
      value_ >>= leb_128_value_bit_shift;
      size += sizeof(int8_t);
    }
    return size;
  }

  inline size_t get_sleb_128_size(int64_t value_)
  {
    size_t size = 0;
    int sign = value_ >> (8 * sizeof(value_) - 1);
    bool isMore = true;
    while (isMore)
    {
      uint8_t byte = value_ & leb_128_value_mask;
      value_ >>= leb_128_value_bit_shift;
      isMore = value_ != sign || ((byte ^ sign) & leb_128_sign_bit_mask) != 0;
      size += sizeof(int8_t);
    }
    return size;
  }

  // return the length in bytes of the encoded value.
  inline size_t encode_sleb_128(int64_t value_,uint8_t *p_)
  {
    uint8_t *p = p_;
    bool isMore = true;
    while (isMore)
    {
      uint8_t byte = value_ & leb_128_value_mask;
      value_ >>= leb_128_value_bit_shift;
      isMore = !((((value_ == 0 ) && ((byte & leb_128_sign_bit_mask) == 0)) ||
                  ((value_ == -1) && ((byte & leb_128_sign_bit_mask) != 0))));
      if (isMore) byte |= leb_128_stop_bit_mask;
      *p++ = byte;
    }
    return (size_t)(p - p_);
  }

  // return the length in bytes of the encoded value.
  inline size_t encode_uleb_128(uint64_t value_,uint8_t *p_)
  {
    uint8_t *p = p_;
    do
    {
      uint8_t byte = value_ & leb_128_value_mask;
      value_ >>= leb_128_value_bit_shift;
      if (value_) byte |= leb_128_stop_bit_mask;
      *p++ = byte;
    }
    while (value_);
    return (size_t)(p - p_);
  }

  inline int decode_uleb_128(const uint8_t *p_,size_t length_,uint64_t& value_)
  {
    const uint8_t *p = p_;
    const uint8_t *pe = p + length_;
    if (p >= pe) return format_error;

    uint64_t value = 0;
    uint32_t shift = 0;
    while (1)
    {
      const uint64_t byte = *p;
      value |= ((byte & leb_128_value_mask) << shift);
      if (byte < leb_128_stop_bit_mask)
      {
        ++p;
        break;
      }
      shift += leb_128_value_bit_shift;
      ++p;
      if (p >= pe) return format_error;
    }
    value_ = value;
    return (int)(p - p_);
  }

  // Smooth 32-bit compilation
  //
  template<typename T>
  inline typename std::enable_if<!std::is_same<T, uint64_t>::value, int>::type
  decode_uleb_128(const uint8_t *p_,size_t length_,T& value_)
  {
    uint64_t value64_ = value_;
    int res = decode_uleb_128(p_, length_, value64_);
    value_= static_cast<T>(value64_);
    return res;
  }

  inline int decode_sleb_128(const uint8_t *p_,size_t length_,int64_t& value_)
  {
    const uint8_t *p = p_;
    const uint8_t *pe = p + length_;
    if (p >= pe) return format_error;

    int64_t value = 0;
    uint32_t shift = 0;
    while (p < pe)
    {
      const uint8_t byte = *p++;
      value |= (static_cast<uint64_t>(byte & leb_128_value_mask) << shift);
      shift += leb_128_value_bit_shift;
      if (byte < leb_128_stop_bit_mask)
      {
        // sign extend negative numbers.
        if ((shift < 8 * sizeof(value)) && (byte & leb_128_sign_bit_mask))
        {
          value |= -((static_cast<int64_t>(1)) << shift);
        }
        value_ = value;
        return (int)(p - p_);
      }
    }
    return format_error;
  }

  inline int decode_sleb_128(const uint8_t *p_,int64_t& value_)
  {
    const uint8_t *p = p_;

    int64_t value = 0;
    uint32_t shift = 0;
    while (1)
    {
      const uint8_t byte = *p++;
      value |= (static_cast<uint64_t>(byte & leb_128_value_mask) << shift);
      shift += leb_128_value_bit_shift;
      if (byte < leb_128_stop_bit_mask)
      {
        // sign extend negative numbers.
        if ((shift < 8 * sizeof(value)) && (byte & leb_128_sign_bit_mask))
        {
          value |= -((static_cast<int64_t>(1)) << shift);
        }
        value_ = value;
        return (int)(p - p_);
      }
    }
    return format_error;
  }

  inline value_type type(const uint8_t *p_,bool& isArray_)
  {
    const uint8_t byte = *p_;
    isArray_ = byte & is_array_mask;
    return (value_type)((byte >> type_shift) & type_mask);
  }

  inline size_t get_leb_128_length(const uint8_t *p_)
  {
    const uint8_t *p = p_;
    while (*p < leb_128_stop_bit_mask) ++p;
    return (p - p_);
  }

// return the number of bytes consumed
  inline int tag_length(const uint8_t *p_,size_t length_,uint64_t& tagLength_)
  {
    const uint8_t byte = *p_;
    tagLength_ = (byte & tag_length_mask);
    return (tagLength_) ? 1 : ((length_ > 1) ? decode_uleb_128(p_ + 1,length_ - 1,tagLength_) : format_error);
  }

  inline int decode_tag(const uint8_t *p_,size_t length_,bflat::string_value& tagValue_)
  {
    const uint8_t byte = *p_;
    size_t tagLength = (byte & tag_length_mask);
    if (tagLength)
    {
      if (length_ < tagLength + 1) return format_error;
      tagValue_._p = reinterpret_cast<const char *>(p_ + 1);
      tagValue_._len  = tagLength;
      return static_cast<int>(tagLength + 1);
    }

    if (length_ < 2) return format_error;

    int ulebByteCount = decode_uleb_128(p_ + 1,length_ - 1,tagLength);
    if (ulebByteCount <= 0) return format_error;

    if (length_ < 1 + ulebByteCount + tagLength) return format_error;
    tagValue_._p = reinterpret_cast<const char *>(p_ + 1 + ulebByteCount);
    tagValue_._len = tagLength;
    return static_cast<int>(1 + ulebByteCount + tagLength);
  }

  inline int decode_null(const uint8_t *p_,size_t length_)
  {
    return 0;
  }

  inline int decode_string(const uint8_t *p_,size_t length_,bflat::string_value& value_)
  {
    size_t slen = 0;
    int ulebByteCount = decode_uleb_128(p_,length_,slen);
    if (ulebByteCount <= 0) return format_error;

    if (length_ < ulebByteCount + slen) return format_error;
    value_._p = reinterpret_cast<const char *>(p_ + ulebByteCount);
    value_._len = slen;
    return static_cast<int>(ulebByteCount + slen);
  }

  inline int decode_binary(const uint8_t *p_,size_t length_,::bflat::string_value& value_)
  {
    size_t blen = 0;
    int ulebByteCount = decode_uleb_128(p_,length_,blen);
    if (ulebByteCount <= 0) return format_error;

    if (length_ < ulebByteCount + blen) return format_error;
    value_._p = reinterpret_cast<const char *>(p_ + ulebByteCount);
    value_._len = blen;
    return static_cast<int>(ulebByteCount + blen);
  }

  inline int decode_int8(const uint8_t *p_,size_t length_,int64_t& value_)
  {
    if (length_ < sizeof(int8_t)) return format_error;
    value_ = *reinterpret_cast<const int8_t *>(p_);
    return sizeof(int8_t);
  }

  inline int decode_int16(const uint8_t *p_,size_t length_,int64_t& value_)
  {
    if (length_ < sizeof(int16_t)) return format_error;
    value_ = *reinterpret_cast<const int16_t *>(p_);
    return sizeof(int16_t);
  }

  inline int decode_int32(const uint8_t *p_,size_t length_,int64_t& value_)
  {
    if (length_ < sizeof(int32_t)) return format_error;
    value_ = *reinterpret_cast<const int32_t *>(p_);
    return sizeof(int32_t);
  }

  inline int decode_int64(const uint8_t *p_,size_t length_,int64_t& value_)
  {
    if (length_ < sizeof(int64_t)) return format_error;
    value_ = *reinterpret_cast<const int64_t *>(p_);
    return sizeof(int64_t);
  }

  inline int decode_double(const uint8_t *p_,size_t length_,double& value_)
  {
    if (length_ < sizeof(double)) return format_error;
    value_ = *reinterpret_cast<const double *>(p_);
    return sizeof(double);
  }

  inline int decode_datetime(const uint8_t *p_,size_t length_,int64_t& value_)
  {
    if (length_ < sizeof(int64_t)) return format_error;
    value_ = *reinterpret_cast<const int64_t *>(p_);
    return sizeof(int64_t);
  }

  inline int decode_leb128(const uint8_t *p_,size_t length_,int64_t& value_)
  {
    return decode_sleb_128(p_,length_,value_);
  }

  inline size_t encode_tag(uint8_t *dst_,uint8_t valueType_,const char *p_,size_t length_)
  {
    uint8_t *dst = dst_;
    if (length_ < 8)
    {
      *dst++ = ((valueType_ << type_shift) | (uint8_t)length_);
      ::memcpy(dst,p_,length_);
      return 1 + length_;
    }
    *dst++ = (valueType_ << type_shift);
    dst += encode_uleb_128(length_,dst);
    ::memcpy(dst,p_,length_);
    dst += length_;
    return dst - dst_;
  }

  inline size_t encode_int8(uint8_t *dst_,int8_t value_)
  {
    *reinterpret_cast<int8_t*>(dst_) = value_;
    return sizeof(int8_t);
  }

  inline size_t encode_int16(uint8_t *dst_,int16_t value_)
  {
    *reinterpret_cast<int16_t*>(dst_) = value_;
    return sizeof(int16_t);
  }

  inline size_t encode_int32(uint8_t *dst_,int32_t value_)
  {
    *reinterpret_cast<int32_t*>(dst_) = value_;
    return sizeof(int32_t);
  }

  inline size_t encode_int64(uint8_t *dst_,int64_t value_)
  {
    *reinterpret_cast<int64_t*>(dst_) = value_;
    return sizeof(int64_t);
  }

  inline size_t encode_double(uint8_t *dst_,double value_)
  {
    *reinterpret_cast<double*>(dst_) = value_;
    return sizeof(double);
  }

  inline size_t encode_leb128(uint8_t *dst_,int64_t value_)
  {
    uint8_t *dst = dst_;
    dst += encode_sleb_128(value_,dst);
    return dst - dst_;
  }

  inline size_t encode_string(uint8_t *dst_,const char *p_,size_t length_)
  {
    uint8_t *dst = dst_;
    dst += encode_uleb_128(length_,dst);
    ::memcpy(dst,p_,length_);
    dst += length_;
    return dst - dst_;
  }

  inline size_t encode_binary(uint8_t *dst_,const char *p_,size_t length_)
  {
    uint8_t *dst = dst_;
    dst += encode_uleb_128(length_,dst);
    ::memcpy(dst,p_,length_);
    dst += length_;
    return dst - dst_;
  }

  inline size_t get_max_tag_size(uint64_t tagLength_)
  {
    return static_cast<size_t>((tagLength_ < 8) ? tagLength_ + 1 : tagLength_ + 1 + get_max_uleb_128_size(tagLength_));
  }

  inline size_t get_max_string_size(uint64_t stringLength_)
  {
    return static_cast<size_t>(get_max_uleb_128_size(stringLength_) + stringLength_);
  }

  struct basic_output_buffer
  {
    static const size_t max_static_buffer_size = 1024;

    basic_output_buffer(void) :
      _allocated(0),
      _data(0L)
    {;}

    ~basic_output_buffer(void)
    {
      if(_data && _data != _static_buffer) ::free(_data);
    }

    inline uint8_t* data(void) const { return _data; }

    inline size_t allocated(void) const { return _allocated; }

    inline uint8_t* resize(size_t byteCount_, size_t length_)
    {
      if(byteCount_ <= _allocated) return _data;

      uint8_t* newBuffer = (uint8_t*)::malloc(byteCount_);
      if(!newBuffer) return NULL;

      ::memcpy(newBuffer,_data,length_);
      _allocated = byteCount_;
      if(_data != _static_buffer) ::free(_data);
      _data = newBuffer;
      return _data;
    }

    size_t _allocated;
    uint8_t* _data;
    uint8_t _static_buffer[max_static_buffer_size];
  };

  /// \brief BFlat message serializer.
  ///
  /// Serializes BFlat messages into a buffer managed by self. To use,
  /// construct a bflat::bflat_serializer and then call one of the "append" methods
  /// for each tag/value pair you wish to encode. Use data() and length()
  /// methods on self to access the encoded form of your data.
  ///
  /// \sa bflat_serializer
  template<class Buffer = basic_output_buffer>
  class bflat_serializer_t
  {
  public:
    /// \brief Constructs a new, empty bflat_serializer_t.
    /// \note In most cases, you can use the typedef bflat_serializer instead of this
    /// template class. bflat_serializer is a typedef for
    /// bflat_serializer_t<bflat_serializer_traits>.
    /// \sa bflat_serializer
    bflat_serializer_t(void)
      : _length(0),
        _p(NULL)
    {
      ;
    }

    /// \brief Destroys the bflat_serializer_t, releasing self's buffer.
    ///
    /// \warning After this serializer is destroyed, the pointer returned by
    /// previous calls to data() or begin() is no longer valid. You must use
    /// or copy your BFlat-encoded data prior to the serializer's destruction.
    ~bflat_serializer_t(void)
    {
    }

    /// \brief Append a tag that begins an array, along with the array length.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param valueType_ The type of data in the array.
    /// \param count_ The count of items in the array.
    /// \warning A call to this method must be followed by "count_" calls
    ///  to append_ of the value type passed-in. Failure to do so results
    ///  in construction of a malformed BFlat message.
    void append_array_tag(const char* pTag_, size_t tagLength_,
                          value_type valueType_, size_t count_)
    {
      _reserve(get_max_tag_size(tagLength_) + get_max_uleb_128_size(count_));
      _length += encode_tag(_p + _length, valueType_ | array_type, pTag_,tagLength_);
      _length += encode_uleb_128(count_,_p+_length);
    }

    /// \brief Append a tag/value pair with a NULL value.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    void append_null(const char *pTag_,size_t tagLength_)
    {
      _reserve(get_max_tag_size(tagLength_));
      _length += encode_tag(_p + _length,bflat::null_type,pTag_,tagLength_);
    }

    /// \brief Append a tag/value pair with a UTF-8 string value
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param p_ A pointer to the UTF-8 encoded string value.
    /// \param length_ The length of the string value, in bytes.
    void append_string(const char *pTag_,size_t tagLength_,const char *p_,size_t length_)
    {
      _reserve(get_max_tag_size(tagLength_) + get_max_string_size(length_));
      _length += encode_tag(_p + _length,bflat::string_type,pTag_,tagLength_);
      _length += encode_string(_p + _length,p_,length_);
    }

    /// \brief Append a string value as an array element.
    /// \param p_ A pointer to the UTF-8 encoded string value.
    /// \param length_ The length of the string value, in bytes.
    void append_string(const char *p_,size_t length_)
    {
      _reserve(get_max_string_size(length_));
      _length += encode_string(_p + _length,p_,length_);
    }


    /// \brief Append a tag/value pair with a binary value.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param p_ A pointer to the binary data.
    /// \param length_ The length of the binary data, in bytes.
    void append_binary(const char *pTag_,size_t tagLength_,const char *p_,size_t length_)
    {
      _reserve(get_max_tag_size(tagLength_) + get_max_string_size(length_));
      _length += encode_tag(_p + _length,bflat::binary_type,pTag_,tagLength_);
      _length += encode_binary(_p + _length,p_,length_);
    }

    /// \brief Append a binary value as an array element.
    /// \param p_ A pointer to the binary data.
    /// \param length_ The length of the binary data, in bytes.
    void append_binary(const char *p_,size_t length_)
    {
      _reserve(get_max_string_size(length_));
      _length += encode_binary(_p + _length,p_,length_);
    }

    /// \brief Append a tag/value pair with a signed 8-bit integer value.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param value_ The value.
    void append_int8(const char *pTag_,size_t tagLength_,int8_t value_)
    {
      _reserve(get_max_tag_size(tagLength_) + sizeof(int8_t));
      _length += encode_tag(_p + _length,bflat::int8_type,pTag_,tagLength_);
      _length += encode_int8(_p + _length,value_);
    }

    /// \brief Append a signed 8-bit value as an array element.
    /// \param value_ The value.
    void append_int8(int8_t value_)
    {
      _reserve(sizeof(int8_t));
      _length += encode_int8(_p + _length,value_);
    }

    /// \brief Append a tag/value pair with a signed 16-bit integer value.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param value_ The value.
    void append_int16(const char *pTag_,size_t tagLength_,int16_t value_)
    {
      _reserve(get_max_tag_size(tagLength_) + sizeof(int16_t));
      _length += encode_tag(_p + _length,bflat::int16_type,pTag_,tagLength_);
      _length += encode_int16(_p + _length,value_);
    }

    /// \brief Append a signed 16-bit value as an array element.
    /// \param value_ The value.
    void append_int16(int16_t value_)
    {
      _reserve(sizeof(int16_t));
      _length += encode_int16(_p + _length,value_);
    }

    /// \brief Append a tag/value pair with a signed 32-bit integer value.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param value_ The value.
    void append_int32(const char *pTag_,size_t tagLength_,int32_t value_)
    {
      _reserve(get_max_tag_size(tagLength_) + sizeof(int32_t));
      _length += encode_tag(_p + _length,bflat::int32_type,pTag_,tagLength_);
      _length += encode_int32(_p + _length,value_);
    }

    /// \brief Append a signed 32-bit value as an array element.
    /// \param value_ The value.
    void append_int32(int32_t value_)
    {
      _reserve(sizeof(int32_t));
      _length += encode_int32(_p + _length,value_);
    }

    /// \brief Append a tag/value pair with a signed 64-bit integer value.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param value_ The value.
    void append_int64(const char *pTag_,size_t tagLength_,int64_t value_)
    {
      _reserve(get_max_tag_size(tagLength_) + sizeof(int64_t));
      _length += encode_tag(_p + _length,bflat::int64_type,pTag_,tagLength_);
      _length += encode_int64(_p + _length,value_);
    }

    /// \brief Append a signed 64-bit value as an array element.
    /// \param value_ The value.
    void append_int64(int64_t value_)
    {
      _reserve(sizeof(int64_t));
      _length += encode_int64(_p + _length,value_);
    }

    /// \brief Append a tag/value pair with a double-precision floating point value.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param value_ The value.
    void append_double(const char *pTag_,size_t tagLength_,double value_)
    {
      _reserve(get_max_tag_size(tagLength_) + sizeof(double));
      _length += encode_tag(_p + _length,bflat::double_type,pTag_,tagLength_);
      _length += encode_double(_p + _length,value_);
    }

    /// \brief Append a double precision floating point value as an array element.
    /// \param value_ The value.
    void append_double(double value_)
    {
      _reserve(sizeof(double));
      _length += encode_double(_p + _length,value_);
    }

    /// \brief Append a tag/value pair with a datetime value, represented
    ///        as a signed 64-bit integer.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param value_ The value.
    void append_datetime(const char *pTag_,size_t tagLength_,int64_t value_)
    {
      _reserve(get_max_tag_size(tagLength_) + sizeof(int64_t));
      _length += encode_tag(_p + _length,bflat::datetime_type,pTag_,tagLength_);
      _length += encode_int64(_p + _length,value_);
    }

    /// \brief Append a datetime 64-bit integer as an array element.
    /// \param value_ The value.
    void append_datetime(int64_t value_)
    {
      _reserve(sizeof(int64_t));
      _length += encode_int64(_p + _length,value_);
    }

    /// \brief Append a tag/value pair with a 64-bit integer value,
    ///        encoded using the variable-length LEB128 encoding.
    /// \param pTag_ A pointer to the UTF-8 encoded tag name.
    /// \param tagLength_ The length of the tag name, in bytes.
    /// \param value_ The value.
    void append_leb128(const char *pTag_,size_t tagLength_,int64_t value_)
    {
      _reserve(get_max_tag_size(tagLength_) + get_max_uleb_128_size(value_));
      _length += encode_tag(_p + _length,bflat::leb128_type,pTag_,tagLength_);
      _length += encode_leb128(_p + _length,value_);
    }

    /// \brief Append a LEB128-encoded integer as an array element.
    /// \param value_ The value.
    void append_leb128(int64_t value_)
    {
      _reserve(get_max_uleb_128_size(value_));
      _length += encode_leb128(_p + _length,value_);
    }

    /// \brief A pointer to the BFlat-encoded message data.
    /// \returns A pointer to self's internal buffer.
    /// \warning This buffer is owned by the bflat serializer; use of this
    /// buffer after the bflat serializer is destroyed will result in
    /// undefined behavior.
    const char *data(void) const
    {
      return reinterpret_cast<const char *>(_p);
    }
    /// \brief The length of the BFlat-encoded data returned by data().
    /// \returns The length of BFlat-encoded data, in bytes.
    size_t length(void) const
    {
      return _length;
    }

    /// \brief A synonym for data().
    const uint8_t *begin(void) const
    {
      return _p;
    }
    /// \brief Returns a pointer to the byte at the end of
    ///        BFlat-encoded data in self's buffer.
    const uint8_t *end(void) const
    {
      return _p + _length;
    }

    /// \brief Clears self's state, so a new message can be serialized.
    void reset(void)
    {
      _length = 0;
    }

    void _reserve(size_t addCount_)
    {
      if (_length + addCount_ > _buffer.allocated())
      {
        size_t newSize = upper_power_of_two(_length + addCount_);
        _p = _buffer.resize(newSize,_length);
      }
    }


    Buffer& buffer(void) { return _buffer; }

    size_t    _length;
    uint8_t  *_p;
    Buffer    _buffer;
  };

  /// \brief The default BFlat serializer type, which uses malloc() and free()
  ///        to manage memory.
  typedef bflat_serializer_t<basic_output_buffer> bflat_serializer;

  /// \brief Represents a single tag/value pair parsed from a BFlat message.
  ///
  /// To use bflat_value, create a bflat_deserializer and use
  /// bflat_deserializer::getNext() to read bflat_value objects from the message.
  /// \note This class is zero-copy for the tag names and values of string and
  ///       binary type. Pointers returned by this class point into the buffer
  ///       from which the value was parsed.
  class bflat_value
  {
  public:
    /// \brief Constructs a bflat_value with a NULL type, no tag, and no value.
    bflat_value(void)
      : _valueType(bflat::null_type),
        _isArray(false),
        _pValue(0),
        _value(0),
        _length(0)
    {
      ;
    }

    /// \brief Decodes a single value from a BFlat message.
    ///
    /// Most applications will use bflat_deserializer::getValue() instead
    /// of this method.
    ///
    /// \param p_ A pointer to the first byte of the value to be decoded.
    /// \param length_ The maximum number of bytes, beginning at p_,
    ///                which may be examined for decoding this value.
    /// \returns 0 if a value is successfully decoded, <0 if an error occurs.
    int decode(const uint8_t *p_,size_t length_)
    {
      const uint8_t *p = p_;
      const uint8_t *pe = p + length_;

      // get the type and array designation
      _valueType = bflat::type(p_,_isArray);

      // get the tag
      int consumedByteCount = bflat::decode_tag(p,length_,_tagString);
      if (consumedByteCount <= 0) return bflat::tag_error;
      p += consumedByteCount;

      if (!_isArray)
      {
        switch (_valueType)
        {
        case bflat::null_type:
          _length = 0;
          break;
        case bflat::int8_type:
          _length = sizeof(int8_t);
          break;
        case bflat::int16_type:
          _length = sizeof(int16_t);
          break;
        case bflat::int32_type:
          _length = sizeof(int32_t);
          break;
        case bflat::int64_type:
          _length = sizeof(int64_t);
          break;
        case bflat::double_type:
          _length = sizeof(double);
          break;
        case bflat::datetime_type:
          _length = sizeof(int64_t);
          break;
        case bflat::leb128_type:
        {
          int rc = bflat::decode_sleb_128(p,_value);
          if (rc <= 0) return bflat::format_error;
          _length = rc;
          break;
        }
        case bflat::string_type:
        case bflat::binary_type:
        {
          int ulebByteCount = bflat::decode_uleb_128(p,pe - p,_length);
          if (ulebByteCount <= 0) return bflat::format_error;
          p += ulebByteCount;
          break;
        }
        default:
          return -1;
        }
        _pValue = p;
        p += _length;
      }
      else
      {
        int ulebByteCount = bflat::decode_uleb_128(p,pe - p,_length);
        if (ulebByteCount <= 0) return bflat::format_error;
        p += ulebByteCount;
        _pValue = p;
        size_t byteCount = 0;
        switch (_valueType)
        {
        case bflat::int8_type:
          byteCount = sizeof(int8_t);
          break;
        case bflat::int16_type:
          byteCount = sizeof(int16_t);
          break;
        case bflat::int32_type:
          byteCount = sizeof(int32_t);
          break;
        case bflat::int64_type:
          byteCount = sizeof(int64_t);
          break;
        case bflat::double_type:
          byteCount = sizeof(double);
          break;
        case bflat::datetime_type:
          byteCount = sizeof(int64_t);
          break;
        default:
          break;
        }
        p += byteCount * _length;
      }
      return static_cast<int>(p - p_);
    }

    /// \brief The tag from this tag/value pair.
    /// \returns A string_value object that points to the tag
    ///          of this tag value pair.
    const string_value& tagString(void) const
    {
      return _tagString;
    }

    /// \brief The value type of this tag/value pair.
    /// \returns One of the constants from enum value_type
    value_type valueType(void) const
    {
      return _valueType;
    }

    /// \brief A type-dependent length indicator of this value.
    ///
    /// \returns For strings,  binary types, and LEB128 values, 
    ///    returns the length of the value in bytes. For arrays, returns
    ///    the count of elements in the array. For other types, returns
    ///    sizeof(native type).
    size_t length(void) const
    {
      return _length;
    }

    /// \brief Indicates if this value represents an array or a scalar.
    /// \returns true if an array, false if a scalar.
    bool isArray(void) const
    {
      return _isArray;
    }

    /// \brief Indicates if this value is NULL.
    ///
    /// \note BFlat has a distinct null type, so this method returns true iff
    ///       valueType() is null_type.
    /// \returns true if this value is NULL, false otherwise.
    bool isNull(void) const
    {
      return _valueType == bflat::null_type;
    }

    /// \brief Indicates if this value is a double.
    /// \returns true if this value is a double, false otherwise.
    bool isDouble(void) const
    {
      return _valueType == bflat::double_type;
    }

    /// \brief Indicates if this value is a string.
    /// \returns true if this value is a string, false otherwise.
    bool isString(void) const
    {
      return _valueType == bflat::string_type;
    }

    /// \brief Indicates if this value is a fixed integer type.
    /// \returns true if this value is int8_type, int16_type,
    /// int32_type or int64_type.
    bool isInt(void) const
    {
      return _valueType >= bflat::int8_type && _valueType <= bflat::int64_type;
    }

    /// \brief Returns a pointer to the first byte of the value's data.
    /// Use this method for direct access to this value's data, when you
    /// would like to decode it yourself. This method is valuable on
    /// little-endian platforms where value data can be cast to a
    /// fixed type without conversion, or for binary types where your
    /// application-level parsing can operate directly on the underlying buffer.
    /// \returns A pointer to the beginning of this value's data.
    /// \sa end()
    const uint8_t* begin(void) const
    {
      return _pValue;
    }

    /// \brief Converts this value to an integer.
    /// \param value_ An output parameter, set to this value if successful.
    /// \returns 0 if successful.
    int getInt(int64_t& value_) const
    {
      if (!_isArray)
      {
        switch (_valueType)
        {
        case bflat::null_type:
          value_ = 0;
          return 0;
        case bflat::int8_type:
          value_ = static_cast<int64_t>(*reinterpret_cast<const int8_t*>(_pValue));
          return 0;
        case bflat::int16_type:
          value_ = static_cast<int64_t>(*reinterpret_cast<const int16_t*>(_pValue));
          return 0;
        case bflat::int32_type:
          value_ = static_cast<int64_t>(*reinterpret_cast<const int32_t*>(_pValue));
          return 0;
        case bflat::int64_type:
          value_ = *reinterpret_cast<const int64_t*>(_pValue);
          return 0;
        case bflat::double_type:
          value_ = static_cast<int64_t>(*reinterpret_cast<const double_t*>(_pValue));
          return 0;
        case bflat::datetime_type:
          value_ = *reinterpret_cast<const int64_t*>(_pValue);
          return 0;
        case bflat::leb128_type:
          value_ = _value;
          return 0;
        default:
          return -1;
        }
      }
      return -1;
    }

    /// \brief Converts this value to a double.
    /// \param value_ An output parameter, set to this value if successful.
    /// \returns 0 if successful.
    int getDouble(double& value_) const
    {
      if (!_isArray)
      {
        switch (_valueType)
        {
        case bflat::null_type:
          value_ = 0;
          return 0;
        case bflat::int8_type:
          value_ = static_cast<double>(*reinterpret_cast<const int8_t*>(_pValue));
          return 0;
        case bflat::int16_type:
          value_ = static_cast<double>(*reinterpret_cast<const int16_t*>(_pValue));
          return 0;
        case bflat::int32_type:
          value_ = static_cast<double>(*reinterpret_cast<const int32_t*>(_pValue));
          return 0;
        case bflat::int64_type:
          value_ = static_cast<double>(*reinterpret_cast<const int64_t*>(_pValue));
          return 0;
        case bflat::double_type:
          value_ = *reinterpret_cast<const double_t*>(_pValue);
          return 0;
        case bflat::datetime_type:
          value_ = static_cast<double>(*reinterpret_cast<const int64_t*>(_pValue));
          return 0;
        case bflat::leb128_type:
          value_ = static_cast<double>(_value);
          return 0;
        default:
          return -1;
        }
      }
      return -1;
    }

    /// \brief Returns this string or binary value.
    /// \param value_ An output parameter which is set to this value's
    ///               string or binary value, if this value is a string or binary.
    /// \note This function does not copy data. The returned string_value
    ///       contains a pointer into the original message buffer.
    /// \returns 0 if successful
    int getString(string_value& value_)
    {
      if (!(_valueType & (bflat::string_type | bflat::binary_type))) return -1;
      value_._p = reinterpret_cast<const char *>(_pValue);
      value_._len = _length;
      return 0;
    }

  protected:
    value_type     _valueType;
    bool           _isArray;
    uint8_t        _pad1[sizeof(size_t) - sizeof(uint8_t) - sizeof(bool)];
    string_value   _tagString;
    const uint8_t *_pValue;
    int64_t        _value;
    size_t         _length; // array length or string length

  }; // class bflat_value

  /// \brief Deserializes a BFlat message into zero or more bflat_value
  ///        objects.
  ///
  /// This is the primary means for parsing or deserializing BFlat messages. To
  /// use, construct a deserializer and pass the raw data bytes. Then, call
  /// getNext() in a loop to read the tag/value pairs. If you encounter an
  /// array of a variable-length type (an array of strings, binary, or leb128),
  /// note that you must also call decode_array_string(),
  /// decode_array_binary(), or decode_array_leb128() to advance through the
  /// values in the array.
  class bflat_deserializer
  {
  public:
    /// \brief Constructs a bflat_deserializer with no raw data.
    ///
    /// \note If you use this constructor, you must call reset() to provide
    ///       data to this deserializer before calling getNext().
    bflat_deserializer(void)
      : _p(0),
        _pe(0)
    {;}

    /// \brief Constructs a bflat_deserializer with a BFlat message.
    ///
    /// \param p_ A pointer to the raw BFlat message data.
    /// \param length_ The length of the message at p_, in bytes.
    bflat_deserializer(const char* p_, size_t length_)
    {
      reset(p_,length_);
    }

    /// \brief Resets self's internal state to begin deserializing
    ///        a new message.
    /// \param p_ A pointer to the raw BFlat message data.
    /// \param length_ The length of the message at p_, in bytes.
    void reset(const char *p_,size_t length_)
    {
      _p = reinterpret_cast<const uint8_t*>(p_);
      _pe = _p + length_;
    }

    /// \brief Retrieve the next value from this message.
    /// \param value_ An output parameter which is filled with the
    ///               value data for the next value from the message.
    /// \returns true if successful, false if no more values exist.
    bool getNext(bflat_value& value_)
    {
      if (_p < _pe)
      {
        int rc = value_.decode(_p,(_pe - _p));
        if (rc > 0)
        {
          _p += rc;
          return true;
        }
      }
      return false;
    }

    /// \brief The current position of this deserializer in the message.
    ///
    /// \return A pointer to the current position of this deserializer.
    const uint8_t* position(void) const
    {
      return _p;
    }

    /// \brief The end of raw message data.
    /// \returns A pointer to the end of the raw BFlat data that is
    ///          being deserialized.
    const uint8_t* end(void) const
    {
      return _pe;
    }

    /// \brief Decodes a LEB128 array element.
    ///
    /// When deserializing a value that is an array of LEB128 values,
    /// call this method to decode each LEB128 array element from the message.
    /// Doing this advances the deserializer through the array elements,
    /// which must be done efore calling getValue() to retrieve the
    /// next tag/value pair after after the array.
    /// \param value_ An output parameter to fill in with the decoded value
    /// \returns true if a value is decoded, false otherwise.
    const bool decode_array_leb128(int64_t& value_)
    {
      if(_p < _pe)
      {
        int rc = decode_leb128(_p, _pe-_p, value_);
        if( rc > 0)
        {
          _p += rc;
          return true;
        }
      }
      return false;
    }
    /// \brief Decodes a string array element.
    ///
    /// When deserializing a value that is an array of strings,
    /// call this method to decode each string array element from the message.
    /// Doing this advances the deserializer through the array elements,
    /// which must be done efore calling getValue() to retrieve the
    /// next tag/value pair after after the array.
    /// \param value_ An output parameter to fill in with the decoded value
    /// \returns true if a value is decoded, false otherwise.
    const bool decode_array_string(string_value& value_)
    {
      if(_p < _pe)
      {
        int rc = decode_string(_p, _pe-_p, value_);
        if( rc > 0)
        {
          _p += rc;
          return true;
        }
      }
      return false;
    }
    /// \brief Decodes a binary array element.
    ///
    /// When deserializing a value that is an array of binaries,
    /// call this method to decode each binary array element from the message.
    /// Doing this advances the deserializer through the array elements,
    /// which must be done efore calling getValue() to retrieve the
    /// next tag/value pair after after the array.
    /// \param value_ An output parameter to fill in with the decoded value
    /// \returns true if a value is decoded, false otherwise.
    const bool decode_array_binary(string_value& value_)
    {
      if(_p < _pe)
      {
        int rc = decode_string(_p, _pe-_p, value_);
        if( rc > 0)
        {
          _p += rc;
          return true;
        }
      }
      return false;
    }
  protected:
    const uint8_t *_p;
    const uint8_t *_pe;

  }; // class bflat_deserializer

} // namespace bflat

#endif
