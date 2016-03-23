README for BFlat Java Library
===============================

Prerequisites
-------------

* Java JDK 1.6 or greater
* Ant 1.9.2 or greater

Installation
------------

Use the supplied `build.xml` file in this directory to build bflat.jar.  Once
build, add bflat.jar to your project and/or to your CLASSPATH to use classes
from the BFlat Java library. API documentation is built automatically when
using ant with this `build.xml` file.

Usage
-----

Add `bflat.jar` to your CLASSPATH when compiling java code that uses BFlat.
For example:

    javac -cp bflat.jar MyBFlatProgram.java


BFlat Java classes are located in the `io.bflat` package.

### Serializing ###

Use `BFlatBuilder` to serialize tag/value pairs into a byte array:

    import io.bflat.*;
    
    ...
    // Build a document
    BFlatBuilder builder(new byte[1024],0);
    builder.encode("price",50.11);
    builder.encode("name","MSFT");
    // Encoded length is in builder.position and the output array
    // is at builder.data.
    byte[] output = builder.data;
    int length = builder.position;

### Deserializing ###

Use `BFlatParser` like an enumerator over the tag/value pairs in your BFlat
document or message:

    import io.bflat.*;

    ...
    // obtain the encoded BFlat message to parse
    byte[] data = ...;
    int length = ...;
    BFlatParser parser = new BFlatParser();
    for(BFlatValue val : parser.parse(data,0,length))
    {
       System.out.println(val);
       // Use "get..." methods on BFlatValue to extract the tag and value.
    }

Tests
-----

Run `ant test` to build and run the BFlat Java test suite. This test suite
depends on JUnit, and will download an appropriate release of JUnit the first
time it is run.


