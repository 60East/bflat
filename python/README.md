README for BFlat Python Extension Module
========================================

Prerequisites
-------------

* Python 2.7 or greater
* C++ compiler compatible with your python installation that also
  supports C++11 features.

Installation
------------

The BFlat Python library is an extension module written in C++, based on
the BFlat C++ library. To build this module use the `setup.py` script in
this directory:

    $ python setup.py build

or

    $ sudo python setup.py install

to install the BFlat module into your python distribution's search path.

Since this module is written in C++, python distutils will attempt to
build the module with your system's C++ compiler, typically g++. You
may need to install additional software or packages for this to be successful.

Usage
-----

Place the parent of the built `bflat` directory in your `PYTHONPATH` after
building, for example:

    $ python setup.py build
    running build
    running build_py
    creating build
    creating build/lib.linux-x86_64-2.7
    creating build/lib.linux-x86_64-2.7/bflat
    copying src/bflat/__init__.py -> build/lib.linux-x86_64-2.7/bflat
    running build_ext
    building 'bflat._bflat_native' extension
    creating build/temp.linux-x86_64-2.7
    creating build/temp.linux-x86_64-2.7/src
    gcc -pthread -fno-strict-aliasing -O2 -g -pipe -Wall -Wp,-D_FORTIFY_SOURCE=2 -fexceptions -fstack-protector-strong --param=ssp-buffer-size=4 -grecord-gcc-switches -m64 -mtune=generic -D_GNU_SOURCE -fPIC -fwrapv -DNDEBUG -O2 -g -pipe -Wall -Wp,-D_FORTIFY_SOURCE=2 -fexceptions -fstack-protector-strong --param=ssp-buffer-size=4 -grecord-gcc-switches -m64 -mtune=generic -D_GNU_SOURCE -fPIC -fwrapv -fPIC -I/usr/include/python2.7 -c src/bflat_python.cpp -o build/temp.linux-x86_64-2.7/src/bflat_python.o -std=c++11
    g++ -pthread -shared -Wl,-z,relro build/temp.linux-x86_64-2.7/src/bflat_python.o -L/usr/lib64 -lpython2.7 -o build/lib.linux-x86_64-2.7/bflat/_bflat_native.so

    $ export PYTHONPATH=build/lib.linux-x86_64-2.7/:$PYTHONPATH
    $ python
    Python 2.7.5 (default, Jun 17 2014, 18:11:42) 
    [GCC 4.8.2 20140120 (Red Hat 4.8.2-16)] on linux2
    Type "help", "copyright", "credits" or "license" for more information.
    >>> import bflat

### Serializing and Deserializing  ###

The BFlat Python extension module provides a simple `dumps()` and `loads()`
interface that converts a Python dictionary to and from a BFlat-encoded
string. The following is a example of interactively using the bflat library:

    >>> import bflat                                                                                                                                                                                   
    >>> data = {"string": "string_data", "int": 9, "double": 10.0, "array": [ "string_value", 99, 100.1, None ] }                                                                                      
    >>> bflat_data = bflat.dumps(data)

    >>> print repr(bflat_data)
     '\x1bint\t>double\x00\x00\x00\x00\x00\x00$@\x8darray\x01\x0cstring_value\x9darray\x01c\xbdarray\x01fffff\x06Y@\x85array\x01\x0estring\x0bstring_data'

    >>> print bflat.loads(bflat_data)
     {'int': 9, 'double': 10.0, 'array': ['string_value', 99, 100.1, None], 'string': 'string_data'}



Tests
-----

Unit tests based on the `nosetests` library are provided in the `tests`
directory.


