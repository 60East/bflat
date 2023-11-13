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
#define PY_SSIZE_T_CLEAN 1
#include <Python.h>

// python 3 support
#if PY_MAJOR_VERSION >= 3
#define IS_PY3X 1
#endif

#include "../../cpp/include/bflat/bflat.hpp"

static const bflat::value_type bflat_unknown_type = (bflat::value_type)-1;

struct bflat_python_string_buffer
{
  bflat_python_string_buffer(void)
    : _pyString(NULL), _allocated(0)
  {;}

#ifdef IS_PY3X
  inline uint8_t* resize(size_t byteCount, size_t length)
  {
    if(!_pyString)
    {
      _pyString = PyBytes_FromStringAndSize(NULL,byteCount);
    }
    else
    {
      _PyBytes_Resize(&_pyString, byteCount);
    }
    _allocated = PyBytes_GET_SIZE(_pyString);
    return (uint8_t*)PyBytes_AS_STRING(_pyString);
  }
#else
  inline uint8_t* resize(size_t byteCount, size_t length)
  {
    if(!_pyString)
    {
      _pyString = PyString_FromStringAndSize(NULL,byteCount);
    }
    else
    {
      _PyString_Resize(&_pyString, byteCount);
    }
    _allocated = PyString_GET_SIZE(_pyString);
    return (uint8_t*)PyString_AS_STRING(_pyString);
  }
#endif

  size_t allocated(void) const { return _allocated; }
  PyObject* pythonString(size_t finalLength)
  {
#ifdef IS_PY3X
    _PyBytes_Resize(&_pyString,finalLength);
#else
    _PyString_Resize(&_pyString,finalLength);
#endif
    return _pyString;
  }

  PyObject* _pyString;
  size_t _allocated;
};
typedef bflat::bflat_serializer_t<bflat_python_string_buffer> py_bflat_serializer;

inline void append_smallest_integer(py_bflat_serializer& serializer,
                               const char* tag, size_t tagLength,
                               int64_t value)
{
  if( value >= INT8_MIN && value <= INT8_MAX)
  {
    serializer.append_int8(tag, tagLength, (int8_t) value);
  }
  else if( value >= INT16_MIN && value <= INT16_MAX)
  {
    serializer.append_int16(tag, tagLength, (int16_t) value);
  }
  else if( value >= INT32_MIN && value <= INT32_MAX)
  {
    serializer.append_int32(tag, tagLength, (int32_t) value);
  }
  else
    serializer.append_int64(tag, tagLength, value);
}

bflat::value_type python_to_bflat_type(PyObject* value, int64_t& int_value)
{
#ifdef IS_PY3X
  if(PyLong_Check(value))
  {
    int_value = PyLong_AsUnsignedLongLongMask(value);
    return bflat::int64_type; // we'll narrow later
  }
#  if !defined(BYTES_AS_STRING) || BYTES_AS_STRING == 0
  else if(PyBytes_Check(value))
  {
    return bflat::binary_type;
  }
#  endif
  else if(PyBytes_Check(value) || PyUnicode_Check(value))
  {
    return bflat::string_type;
  }
#else
  if(PyInt_Check(value))
  {
    int_value = PyNumber_AsSsize_t(value,NULL);
    return bflat::int64_type; // we'll narrow later
  }
  else if(PyString_Check(value) || PyUnicode_Check(value))
  {
    return bflat::string_type;
  }
#endif
  else if(PyLong_Check(value))
  {
    int_value = PyLong_AsUnsignedLongLongMask(value);
    return bflat::int64_type;
  }
  else if(PyFloat_Check(value))
  {
    return bflat::double_type;
  }
  else if(PyBool_Check(value))
  {
    int_value = value == Py_None;
    return bflat::int64_type;
  }
  else if(value==Py_None)
  {
    return bflat::null_type;
  }
  PyErr_SetString(PyExc_ValueError,"Only flat dictionaries and arrays can be serialized with BFlat.");
  return bflat_unknown_type;
}

bflat::value_type python_to_bflat_array_slice(PyObject **array,
    size_t remaining, size_t& slice_length)
{
  bflat::value_type thisType = bflat_unknown_type;
  int64_t min=0, max=0;
  while(remaining--)
  {
    int64_t int_value = 0;
    bflat::value_type type = python_to_bflat_type(*array++,int_value);
    if(type == bflat_unknown_type) return type;
    if(thisType != bflat_unknown_type && type != thisType) break; // done with this slice
    thisType = type;
    ++slice_length;
    if(type == bflat::int64_type)
    {
      if(int_value < min) min = int_value;
      else if(int_value > max) max = int_value;
    }
  }

  // now narrow if this was an integer.
  if(thisType == bflat::int64_type)
  {
    if(min >= INT8_MIN && max <= INT8_MAX) thisType = bflat::int8_type;
    else if(min >= INT16_MIN && max <= INT16_MAX) thisType = bflat::int16_type;
    else if(min >= INT32_MIN && max <= INT32_MAX) thisType = bflat::int32_type;
  }
  return thisType;
}

#ifdef IS_PY3X
class string_reference
{
  public:
  string_reference(PyObject* value)
    : _data(NULL), _length(0), _pyobject(NULL)
  {
    if(PyBytes_Check(value))
    {
      _data = PyBytes_AS_STRING(value);
      _length = PyBytes_GET_SIZE(value);
    }
    else if(PyUnicode_Check(value))
    {
      _pyobject = PyUnicode_AsUTF8String(value);
      _data = PyBytes_AS_STRING(_pyobject);
      _length = PyBytes_GET_SIZE(_pyobject);
    }
    else
    {
      _pyobject = PyObject_Str(value);
      _data = PyBytes_AS_STRING(_pyobject);
      _length = PyBytes_GET_SIZE(_pyobject);
    }
  }
  ~string_reference()
  {
    Py_XDECREF(_pyobject);
  }
  const char* data(void) const { return _data; }
  size_t length(void) const { return _length;}
  private:
  string_reference& operator=(const string_reference&);
  string_reference(const string_reference&);

  const char* _data;
  size_t      _length;
  PyObject*   _pyobject;
};
#else
class string_reference
{
  public:
  string_reference(PyObject* value)
    : _data(NULL), _length(0), _pyobject(NULL)
  {
    if(PyString_Check(value))
    {
      _data = PyString_AS_STRING(value);
      _length = PyString_GET_SIZE(value);
    }
    else if(PyUnicode_Check(value))
    {
      _pyobject = PyUnicode_AsUTF8String(value);
      _data = PyString_AS_STRING(_pyobject);
      _length = PyString_GET_SIZE(_pyobject);
    }
    else
    {
      _pyobject = PyObject_Str(value);
      _data = PyString_AS_STRING(_pyobject);
      _length = PyString_GET_SIZE(_pyobject);
    }
  }
  ~string_reference()
  {
    Py_XDECREF(_pyobject);
  }
  const char* data(void) const { return _data; }
  size_t length(void) const { return _length;}
  private:
  string_reference& operator=(const string_reference&);
  string_reference(const string_reference&);

  const char* _data;
  size_t      _length;
  PyObject*   _pyobject;
};
#endif

int64_t extractInt(PyObject* value)
{
#ifndef IS_PY3X
  if(PyInt_Check(value))
    return PyNumber_AsSsize_t(value,NULL);
#endif
  if(PyLong_Check(value))
    return PyLong_AsUnsignedLongLongMask(value);
  if(PyBool_Check(value))
    return value == Py_True ? 1 : 0;
  assert(false);
  return 0;
}

double extractDouble(PyObject* value)
{
  if(PyFloat_Check(value))
    return PyFloat_AS_DOUBLE(value);
  assert(false);
  return 0;
}


bool append_array(py_bflat_serializer& serializer,
                  const char* tag, size_t tagLength,
                  PyObject* value)
{
  PyObject* fastSequence = PySequence_Fast(value, NULL);
  size_t count = PySequence_Fast_GET_SIZE(fastSequence);
  size_t index = 0;
  PyObject** array = PySequence_Fast_ITEMS(fastSequence);

  // special case for empty arrays
  if(!count)
  {
    serializer.append_array_tag(tag,tagLength,bflat::null_type,0);
  }
  else while(index<count)
  {
    // Find homogenous slices and serialize them.
    size_t slice_length = 0;
    bflat::value_type slice_type = python_to_bflat_array_slice(array+index,
        count-index, slice_length);
    serializer.append_array_tag(tag,tagLength,slice_type,slice_length);
    if(slice_type == bflat_unknown_type)
    {
      // Abort
      Py_DECREF(fastSequence);
      return false;
    }
    switch(slice_type)
    {
      case bflat::int8_type:
        for(size_t subindex=0;subindex<slice_length;++subindex)
          serializer.append_int8((int8_t)extractInt(array[index+subindex]));
        break;
      case bflat::int16_type:
        for(size_t subindex=0;subindex<slice_length;++subindex)
          serializer.append_int16((int16_t)extractInt(array[index+subindex]));
        break;
      case bflat::int32_type:
        for(size_t subindex=0;subindex<slice_length;++subindex)
          serializer.append_int32((int32_t)extractInt(array[index+subindex]));
        break;
      case bflat::int64_type:
        for(size_t subindex=0;subindex<slice_length;++subindex)
          serializer.append_int64((int64_t)extractInt(array[index+subindex]));
        break;
      case bflat::double_type:
        for(size_t subindex=0;subindex<slice_length;++subindex)
          serializer.append_double(extractDouble(array[index+subindex]));
        break;
#if !defined(BYTES_AS_STRING) || BYTES_AS_STRING == 0
      case bflat::binary_type:
        for(size_t subindex=0;subindex<slice_length;++subindex)
        {
          string_reference string_value(array[index+subindex]);
          serializer.append_binary(string_value.data(),string_value.length());
        }
        break;
#endif
      case bflat::string_type:
        for(size_t subindex=0;subindex<slice_length;++subindex)
        {
          string_reference string_value(array[index+subindex]);
          serializer.append_string(string_value.data(),string_value.length());
        }
        break;
      case bflat::null_type:
        // nothing to do;
        break;
      default:
        assert(false);
        break;
    }
    index += slice_length;
  }
  Py_DECREF(fastSequence);
  return true;
}

static PyObject* bflat_native_dumps(PyObject *self, PyObject* args)
{
  PyObject* dict;

  if(!PyArg_ParseTuple(args, "O", &dict))
  {
    return NULL;
  }

  if (!PyDict_Check(dict))
  {
    PyErr_SetString(PyExc_TypeError, "dumps: argument is not a dict");
    return NULL;
  }

  py_bflat_serializer serializer;
  PyObject *key, *value;
  Py_ssize_t pos =0;
  while(PyDict_Next(dict, &pos, &key, &value))
  {
    const char* key_string = NULL;
    Py_ssize_t key_length = 0;
    PyObject *new_key = NULL;

#ifdef IS_PY3X
    if(PyUnicode_Check(key))
    {
      key_string = PyUnicode_AsUTF8AndSize(key, &key_length);
    }
#else
    if(PyString_Check(key))
    {
      key_string = PyString_AS_STRING(key);
      key_length = PyString_GET_SIZE(key);
    }
#endif
    else
    {
      new_key = PyObject_Str(key);
      if(!new_key) continue; // skip this pair
#ifdef IS_PY3X
      key_string = PyUnicode_AsUTF8AndSize(new_key, &key_length);
#else
      key_string = PyString_AS_STRING(new_key);
      key_length = PyString_GET_SIZE(new_key);
#endif
    }

#ifdef IS_PY3X
    if(PyLong_Check(value))
    {
      int64_t long_value = (int64_t)PyLong_AsUnsignedLongLongMask(value);
      append_smallest_integer(serializer,key_string,key_length,long_value);
    }
    else if(PyUnicode_Check(value))
    {
      const char *string_value = nullptr;
      Py_ssize_t length = 0;
      string_value = PyUnicode_AsUTF8AndSize(value, &length);
      serializer.append_string(key_string, key_length, string_value, length);
    }
#  if !defined(BYTES_AS_STRING) || BYTES_AS_STRING == 0
    else if(PyBytes_Check(value))
    {
      char *binary_value = nullptr;
      Py_ssize_t length = 0;
      PyBytes_AsStringAndSize(value, &binary_value, &length);
      serializer.append_binary(key_string, key_length, binary_value, length);
    }
#  endif
#else
    if(PyInt_Check(value))
    {
      long long_value = PyInt_AS_LONG(value);
      append_smallest_integer(serializer,key_string,key_length,long_value);
    }
    else if(PyString_Check(value))
    {
      const char *string_value = PyString_AS_STRING(value);
      Py_ssize_t length = PyString_GET_SIZE(value);
      serializer.append_string(key_string, key_length, string_value, length);
    }
#endif
    else if(PyUnicode_Check(value))
    {
      PyObject* pyUtf8String = PyUnicode_AsUTF8String(value);
#ifdef IS_PY3X
      const char *string_value = nullptr;
      Py_ssize_t length = 0;
      string_value = PyUnicode_AsUTF8AndSize(value, &length);
#else
      const char *string_value = PyString_AS_STRING(pyUtf8String);
      Py_ssize_t length = PyString_GET_SIZE(pyUtf8String);
#endif
      serializer.append_string(key_string, key_length, string_value, length);
      Py_DECREF(pyUtf8String);
    }
    else if(PyFloat_Check(value))
    {
      double double_value = PyFloat_AS_DOUBLE(value);
      serializer.append_double(key_string, key_length, double_value);
    }
    else if(PyLong_Check(value))
    {
      // avoid error checking here, and encode as a 64-bit integer
      int64_t int64_value = (int64_t)PyLong_AsUnsignedLongLongMask(value);
      append_smallest_integer(serializer,key_string,key_length,int64_value);
    }
    else if(PyBool_Check(value))
    {
      bool bool_value = (value == Py_True);
      serializer.append_int8(key_string, key_length, bool_value);
    }
    else if(PySequence_Check(value))
    {
      if(!append_array(serializer,key_string,key_length,value))
      {
        Py_XDECREF(new_key);
        return NULL;
      }
    }
    else if(value == Py_None)
    {
      serializer.append_null(key_string,key_length);
    }
    else
    {
      PyErr_SetString(PyExc_ValueError,"Only flat dictionaries and arrays can be serialized with BFlat.");
      Py_XDECREF(new_key);
      return NULL;
    }
    Py_XDECREF(new_key);
  }

//  return PyString_FromStringAndSize(serializer.data(), serializer.length());
  return serializer.buffer().pythonString(serializer.length());
}

PyObject* toPythonValue(const bflat::bflat_value& value)
{
  int64_t int_value=0;
  double double_value=0;
  switch(value.valueType())
  {
    case bflat::null_type:
      Py_INCREF(Py_None);
      return Py_None;
    case bflat::int8_type:
    case bflat::int16_type:
    case bflat::int32_type:
    case bflat::int64_type:
    case bflat::leb128_type:
    case bflat::datetime_type:
      value.getInt(int_value);
      if(int_value >= LONG_MIN && int_value <= LONG_MAX)
#ifdef IS_PY3X
        return PyLong_FromLong((long)int_value);
#else
        return PyInt_FromLong((long)int_value);
#endif

      else
        return PyLong_FromLongLong(int_value);
    case bflat::string_type:
      //return PyUnicode_FromStringAndSize((const char*)value.begin(),value.length());
#if defined(BYTES_AS_STRING) && BYTES_AS_STRING != 0
    case bflat::binary_type:
#endif
#ifdef IS_PY3X
      return PyUnicode_FromStringAndSize((const char*)value.begin(),value.length());
#else
      return PyString_FromStringAndSize((const char*)value.begin(),value.length());
#endif
#if !defined(BYTES_AS_STRING) || BYTES_AS_STRING == 0
    case bflat::binary_type:
#  ifdef IS_PY3X
      return PyBytes_FromStringAndSize((const char*)value.begin(),value.length());
#  else
      return PyString_FromStringAndSize((const char*)value.begin(),value.length());
#  endif
#endif
    case bflat::double_type:
      value.getDouble(double_value);
      return PyFloat_FromDouble(double_value);
    default:
      assert(false);
      Py_INCREF(Py_None);
      return Py_None;
  }
}

template<class T>
void fixed_array_insert(PyObject* list, bflat::bflat_value& value)
{
  const T* begin = (const T*)value.begin();
  const T* end   = begin + value.length();

  for(;begin!=end;++begin)
  {
    int64_t int_value = *begin;
    PyObject *pyValue;
    if(sizeof(T) > sizeof(long))
    {
      if(int_value >= LONG_MIN && int_value <= LONG_MAX)
      {
#ifdef IS_PY3X        
        pyValue = PyLong_FromLong((long)int_value);
#else
        pyValue = PyInt_FromLong((long)int_value);
#endif
      }
      else
      {
        pyValue = PyLong_FromLongLong(int_value);
      }
    }
    else
    {
#ifdef IS_PY3X        
      pyValue = PyLong_FromLong((long)int_value);
#else
      pyValue = PyInt_FromLong((long)int_value);
#endif
    }
    PyList_Append(list,pyValue);
    Py_DECREF(pyValue);
  }
}
void double_array_insert(PyObject* list, bflat::bflat_value& value)
{
  const double* begin = (const double*)value.begin();
  const double* end   = begin + value.length();

  for(;begin!=end;++begin)
  {
    double double_value = *begin;
    PyObject *pyValue = PyFloat_FromDouble(double_value);
    PyList_Append(list,pyValue);
    Py_DECREF(pyValue);
  }
}

void leb128_array_insert(PyObject* list,
    bflat::bflat_deserializer& deserializer, size_t count)
{
  for(size_t i = 0; i < count; ++i)
  {
    int64_t value;
    PyObject *pyValue;
    if(deserializer.decode_array_leb128(value))
    {
      if(value >= LONG_MIN && value <= LONG_MAX)
#ifdef IS_PY3X
        pyValue = PyLong_FromLong((long)value);
#else
        pyValue = PyInt_FromLong((long)value);
#endif
      else
        pyValue = PyLong_FromLongLong(value);
      PyList_Append(list,pyValue);
      Py_DECREF(pyValue);
    }
  }
}
void string_array_insert(PyObject* list,
    bflat::bflat_deserializer& deserializer, size_t count)
{
  for(size_t i = 0; i < count; ++i)
  {
    bflat::string_value value;
    PyObject *pyValue;
    if(deserializer.decode_array_string(value))
    {
      //pyValue = PyUnicode_FromStringAndSize(value.data(),value.length());
#ifdef IS_PY3X
      pyValue = PyUnicode_FromStringAndSize(value.data(),value.length());
#else
      pyValue = PyString_FromStringAndSize(value.data(),value.length());
#endif
      PyList_Append(list,pyValue);
      Py_DECREF(pyValue);
    }
  }
}
void binary_array_insert(PyObject* list,
    bflat::bflat_deserializer& deserializer, size_t count)
{
  for(size_t i = 0; i < count; ++i)
  {
    bflat::string_value value;
    PyObject *pyValue;
    if(deserializer.decode_array_string(value))
    {
#ifdef IS_PY3X
#  if !defined(BYTES_AS_STRING) || BYTES_AS_STRING == 0
      pyValue = PyBytes_FromStringAndSize(value.data(),value.length());
#  else
      pyValue = PyUnicode_FromStringAndSize(value.data(),value.length());
#  endif
#else
      pyValue = PyString_FromStringAndSize(value.data(),value.length());
#endif
      PyList_Append(list,pyValue);
      Py_DECREF(pyValue);
    }
  }
}


static PyObject* bflat_native_loads(PyObject* self, PyObject* args)
{
  char quick_lookup[256] = {0};
  PyObject *dict;
  const char* data;
  Py_ssize_t length;

  if(!PyArg_ParseTuple(args, "s#", &data, &length))
  {
    return NULL;
  }

  dict = PyDict_New();

  bflat::bflat_deserializer parser(data,length);
  bflat::bflat_value value;

  while(parser.getNext(value))
  {
    auto tagString = value.tagString();
    //PyObject* pyTagString = PyUnicode_FromStringAndSize(tagString.data(),
    //    tagString.length());
    if(!tagString.length()) continue;
#ifdef IS_PY3X
    PyObject* pyTagString = PyUnicode_FromStringAndSize(tagString.data(), tagString.length());
#else
    PyObject* pyTagString = PyString_FromStringAndSize(tagString.data(), tagString.length());
#endif
    if (!pyTagString)
    {
      PyErr_SetString(PyExc_ValueError, "Invalid UTF-8 encoding for tag name");
      return NULL;
    }

    if(!value.isArray())
    {
      PyObject* pyValue = toPythonValue(value);
      if(pyValue)
      {
        PyObject* previous;
        if(quick_lookup[(int)tagString.data()[0]] &&
          (previous = PyDict_GetItem(dict, pyTagString)))
        {
          if (PyList_Check(previous))
          {
            PyList_Append(previous,pyValue);
          }
          else
          {
            PyObject* newList = PyList_New(0);
            PyList_Append(newList, previous);
            PyList_Append(newList, pyValue);
            PyDict_SetItem(dict, pyTagString, newList);
            Py_DECREF(newList);
          }
        }
        else
        {
          quick_lookup[(int)tagString.data()[0]] = 1;
          PyDict_SetItem(dict, pyTagString, pyValue);
        }
        Py_DECREF(pyValue);
      }
    }
    else
    {
      size_t length = value.length();
      PyObject* previous, *list;
      PyObject* newlist = NULL;
      if(quick_lookup[(int)tagString.data()[0]] &&
          (previous = PyDict_GetItem(dict,pyTagString)))
      {
        if(PyList_Check(previous)) list=previous;
        else
        {
          newlist = list = PyList_New(0);
          PyList_Append(list,previous);
        }
      }
      else
      {
        quick_lookup[(int)tagString.data()[0]] = 1;
        newlist = list = PyList_New(0);
      }
      switch(value.valueType())
      {
        case bflat::null_type:
          for(size_t i=0;i<length;++i)
          {
            PyList_Append(list,Py_None);
          }
          break;
        case bflat::int8_type:
          fixed_array_insert<int8_t>(list,value);
          break;
        case bflat::int16_type:
          fixed_array_insert<int16_t>(list,value);
          break;
        case bflat::int32_type:
          fixed_array_insert<int32_t>(list,value);
          break;
        case bflat::int64_type:
        case bflat::datetime_type:
          fixed_array_insert<int64_t>(list,value);
          break;
        case bflat::double_type:
          double_array_insert(list,value);
          break;
        case bflat::string_type:
          string_array_insert(list,parser,length);
          break;
        case bflat::binary_type:
          binary_array_insert(list,parser,length);
          break;
        case bflat::leb128_type:
          leb128_array_insert(list,parser,length);
          break;
      }
      if(newlist)
      {
        PyDict_SetItem(dict, pyTagString, newlist);
        Py_DECREF(newlist);
      }
    }
    Py_DECREF(pyTagString);
  }
  return dict;
}

static PyObject* bflat_native_version(PyObject*, PyObject*)
{
  return PyFloat_FromDouble(bflat::version);
}

static bool bflat_add_constants(PyObject* self)
{
  if(PyModule_AddIntConstant(self,
                             "BYTES_AS_STRING",
#if defined(BYTES_AS_STRING)
                             BYTES_AS_STRING
#else
                             0
#endif
                             ) != 0)
  {
    return false;
  }

  return true;
}

static PyMethodDef bflat_native_methods[] = {
  {"dumps", bflat_native_dumps, METH_VARARGS,
   "Convert a python mapping object to a BFlat string."},
  {"loads", bflat_native_loads, METH_VARARGS,
   "Convert a BFlat string to a python dictionary."},
  {"version", bflat_native_version, METH_VARARGS,
   "Returns the implementation version."},
  {NULL}
};

const char* MODULE_NAME = "_bflat_native";
#ifdef IS_PY3X
  // this struct is needed for python 3 initialization
  static struct PyModuleDef ampsbflat_moduledef = 
  {
    PyModuleDef_HEAD_INIT,      // always this
    MODULE_NAME,                // name of module
    NULL,                       // docstring for module
    0,                          // module state
    bflat_native_methods,       // methods
    NULL,                       // slots (we dont use)
    NULL,                       // traverse (we dont use)
    NULL,                       // clear (we dont use)
    NULL                        // free (we dont use)
  };
#endif

#if IS_PY3X
  PyMODINIT_FUNC PyInit__bflat_native(void)
  {
    PyObject* self = PyModule_Create(&ampsbflat_moduledef);

    if(!bflat_add_constants(self))
    {
      Py_XDECREF(self);
      self = NULL;
    }

    return self;
  }
#else
  PyMODINIT_FUNC init_bflat_native(void)
  {
    PyObject* self = Py_InitModule3(MODULE_NAME,
                                    bflat_native_methods,
                                    "Internal methods for BFlat Python module.");
    bflat_add_constants(self);
  }
#endif

