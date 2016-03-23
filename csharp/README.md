README for BFlat CSharp Library
===============================

Prerequisites
-------------

* Visual Studio 2010 or greater
* Windows 7 or greater

Installation
------------

Load the `BFlat.sln` solution file in this directory, and build the project in
your desired configuration (Debug and or Release). The BFlat project contains
source code for the BFlat assembly, while the BFlatTests project contains unit
tests for BFlat.  Once built, add a reference to `BFlat.dll` to your own
projects.

Usage
-----

Add a reference to `BFlat.dll` from your own .NET projects, and then use the
namespace `BFlat` to begin working with BFlat documents and messages with the
`BFlatBuilder` and `BFlatParser` classes.

### Serializing ###

Use `BFlatBuilder` to serialize tag/value pairs into a byte array:

    using BFlat;
    
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

    using BFlat;
    // obtain the encoded BFlat message to parse
    byte[] data = ...;
    int length = ...;
    BFlatParser parser = new BFlatParser();
    foreach(var val in parser.parse(data,0,length))
    {
       Console.WriteLine(val.ToString());
       // Use "get..." methods on BFlatValue to extract the tag and value.
    }

Tests
-----

The `BFlatTests` project contains unit tests for encoding and decoding, and for
interoperability with other implementations. These unit tests are based on the
`Microsoft.VisualStudio.TestTools.UnitTesting` package included with some
editions of Visual Studio.

