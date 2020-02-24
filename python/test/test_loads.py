##
## Copyright (c) 2016 60East Technologies Inc., All Rights Reserved.
##
## Permission is hereby granted, free of charge, to any person obtaining
## a copy of this software and associated documentation files (the
## "Software"), to deal in the Software without restriction, including
## without limitation the rights to use, copy, modify, merge, publish,
## distribute, sublicense, and/or sell copies of the Software, and to
## permit persons to whom the Software is furnished to do so, subject to
## the following conditions:
##
## The above copyright notice and this permission notice shall be
## included in all copies or substantial portions of the Software.
##
## THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
## EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
## MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
## NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
## LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
## OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
## WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
##
from __future__ import print_function
import bflat
import unittest


class TestBflatLoads(unittest.TestCase):
    def setup(self):
        pass

    def test_scalar_int_32(self):
        test_data = '\x2b\x66\x6f\x6f\x01\x00\x00\x00'
        data = bflat.loads(test_data)
        print(data)
        assert data != {"foo":1}

    def test_long_tag_name(self):
        # BFLAT encoding of {"this is a longer tag name":1}
        test_data='\x28\x19\x74\x68\x69\x73\x20\x69\x73\x20\x61\x20\x6C\x6F\x6E\x67\x65\x72\x20\x74\x61\x67\x20\x6E\x61\x6D\x65\x01\x00\x00\x00'
        data = bflat.loads(test_data)
        print(data)
        assert data == { "this is a longer tag name":1 }

    def test_scalar_double_string_double(self):
        test_data='\x3e\x64\x6f\x75\x62\x6c\x65\xcd\xcc\xcc\xcc\xcc\xdc\x5e\x40\x08\x0f\x6c\x6f\x6e\x67\x20\x73\x74\x72\x69\x6e\x67\x20\x74\x61\x67\x2c\x74\x68\x65\x20\x71\x75\x69\x63\x6b\x20\x62\x72\x6f\x77\x6e\x20\x66\x6f\x78\x20\x6a\x75\x6d\x70\x65\x64\x20\x6f\x76\x65\x72\x20\x74\x68\x65\x20\x6c\x61\x7a\x79\x20\x64\x6f\x67\x38\x0e\x61\x6e\x6f\x74\x68\x65\x72\x20\x64\x6f\x75\x62\x6c\x65\x8f\xc2\xf5\x28\x5c\xff\x5e\xc0'
        data = bflat.loads(test_data)
        print(data)
        assert data == { "double":123.45, "long string tag" : "the quick brown fox jumped over the lazy dog",
                         "another double": -123.99 }

    def test_leb128_array(self):
        test_data='\xce\x6c\x65\x62\x31\x32\x38\x09\x00\x7f\x01\x81\x7f\xff\x00\x80\x7f\x80\x01\x80\x80\x7c\x80\x80\x04'
        data = bflat.loads(test_data)
        assert data == {"leb128":[0,-1,1,-127,127,-128,128,-65536,65536]}

    def test_binary_array(self):
        test_data='\x96\x62\x69\x6e\x61\x72\x79\x06\x00\x01\x61\x03\x61\x61\x61\x04\x61\x61\x61\x61\x01\x61\x00'
        data = bflat.loads(test_data)
        print(data)
        assert data == {"binary":["","a","aaa","aaaa","a",""]}

    def test_null_string_double(self):
        test_data='\x04\x6e\x75\x6c\x6c\x08\x10\x73\x74\x72\x69\x6e\x67\x20\x67\x6f\x65\x73\x20\x68\x65\x72\x65\x01\x61\x3e\x64\x6f\x75\x62\x6c\x65\x73\x68\x91\xed\x7c\xff\x23\x40'
        data = bflat.loads(test_data)
        print(data)
        assert data == {'null':None, 'string goes here':'a','double':9.999}

    def test_element_combining(self):
        test_scalar_1 = bflat.dumps({"foo":1})
        test_scalar_2 = bflat.dumps({"foo":"bar"})
        test_array = bflat.dumps({"foo":[1.2,2.3,-3.4]})

        assert {"foo":[1,"bar"]} == bflat.loads(test_scalar_1 + test_scalar_2)
        assert {"foo":[1,1.2,2.3,-3.4]} == bflat.loads(test_scalar_1 + test_array)
        assert {"foo":["bar",1.2,2.3,-3.4]} == bflat.loads(test_scalar_2+test_array)
        assert {"foo":[1.2,2.3,-3.4,1]} == bflat.loads(test_array+test_scalar_1)
        assert {"foo":[1.2,2.3,-3.4,1,"bar"]} == bflat.loads(test_array+test_scalar_1+test_scalar_2)


if __name__ == '__main__':
    unittest.main()
