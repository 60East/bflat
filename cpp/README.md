README for BFlat C++ Library
============================

Prerequisites
-------------

* g++ 4.8 or greater, or another compiler with c++11 support
* CMake 2.6.2 or greater, if you wish to compile and run unit tests. 
* Doxygen 1.8.11 or greater, if you wish to build API documentation.
* 
Installation
------------

The BFlat C++ library is just a header file, so to use it, you do not need to
configure or build anything. Just add the "include" directory in this
distribution to your compiler's include path:


    g++ -I include/ -c foo.cpp

and then you can use the symbols from the `bflat` namespace.

API Documentation
-----------------
  
A `Doxyfile` is provided to use with doxygen for API documentation. The
generated API documentation contains a complete reference for the library
and examples to help you get started.


Usage
-----

To use the library, include `bflat/bflat.hpp`, and then use 
`bflat::bflat_serializer` to create BFlat messages, and
`bflat::bflat_deserializer` to decode BFlat messages.

### Serializing

To serialize a BFlat message, construct a `bflat::bflat_serializer` and
use the `append` methods to add tag/value pairs and arrays to the document:

    bflat_serializer serializer;
    serializer.append_int32("id",strlen("id"),12345);
    const char* name = ...;
    size_t name_length = strlen(name);
    serializer.append_string("name",strlen("name"), name, name_length);
    ...
    const char* buffer = serializer.data();
    size_t buffer_length = serializer.length();
    
The `bflat_serializer` manages an internal buffer where BFlat-encoded data is
appended. To access the BFlat-encoded data, use the `data()` and `length()`
methods.

### Deserializing

To deserialize a BFlat message, use `bflat::bflat_deserializer` which acts like
an iterator over the tag/value pairs in a message:

    bflat_deserializer deserializer(raw_data, raw_data_length);
    bflat_value value;
    while(deserialzer.getNext(value))
    {
        ...  
    }
    
`bflat_value` is a type that represents a single tag/value pair in a parsed
BFlat message. Use the `valueType()` method or the `isNull()`, `isDouble()`,
`isString()` and `isInt()` methods to determine the type of the value,
and use the `tagString()` method to get the tag name. Based on the type
of the value, use the `getDouble()`, `getString()`, or `getInt()` methods
to retrieve the value.

See the generated API documentation for more detailed examples.

Tests
-----

The `test` tree contains the test suite for this library. To use it, obtain
and build the GoogleTest framework from http://github.com/google/googletest.
A shell script `get_googletest.sh` is provided to download and build the google
test framework (requires CMake 2.6.2 or greater).

Run `make` to build the test suite:

    $ make
    g++ -O0 -g -std=c++11 -Igoogletest-master/googletest/include -I../include   -c -o test_leb128.o test_leb128.cpp
    g++ -O0 -g -std=c++11 -Igoogletest-master/googletest/include -I../include   -c -o test_parser.o test_parser.cpp
    g++ -O0 -g -std=c++11 -Igoogletest-master/googletest/include -I../include   -c -o test_serializer.o test_serializer.cpp
    g++ -o bflat_tests test_leb128.o test_parser.o test_serializer.o -Lgoogletest-master/googletest/build -lgtest_main -lgtest -lpthread
    
And then run `bflat_tests` to run the test suite.





